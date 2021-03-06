﻿#if NET20 || NET30

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection.Emit;
using Theraot.Core;

namespace System.Linq.Expressions.Compiler
{
    internal partial class LambdaCompiler
    {
        #region Conditional

        private void EmitConditionalExpression(Expression expr, CompilationFlags flags)
        {
            var node = (ConditionalExpression)expr;
            Debug.Assert(node.Test.Type == typeof(bool));
            var labFalse = _ilg.DefineLabel();
            EmitExpressionAndBranch(false, node.Test, labFalse);
            EmitExpressionAsType(node.IfTrue, node.Type, flags);

            if (NotEmpty(node.IfFalse))
            {
                var labEnd = _ilg.DefineLabel();
                if ((flags & CompilationFlags.EmitAsTailCallMask) == CompilationFlags.EmitAsTail)
                {
                    // We know the conditional expression is at the end of the lambda,
                    // so it is safe to emit Ret here.
                    _ilg.Emit(OpCodes.Ret);
                }
                else
                {
                    _ilg.Emit(OpCodes.Br, labEnd);
                }
                _ilg.MarkLabel(labFalse);
                EmitExpressionAsType(node.IfFalse, node.Type, flags);
                _ilg.MarkLabel(labEnd);
            }
            else
            {
                _ilg.MarkLabel(labFalse);
            }
        }

        private static bool NotEmpty(Expression node)
        {
            var empty = node as DefaultExpression;
            if (empty == null || empty.Type != typeof(void))
            {
                return true;
            }

            return false;
        }

        private static bool Significant(Expression node)
        {
            var block = node as BlockExpression;
            if (block != null)
            {
                for (var i = 0; i < block.ExpressionCount; i++)
                {
                    if (Significant(block.GetExpression(i)))
                    {
                        return true;
                    }
                }
                return false;
            }
            return NotEmpty(node) && !(node is DebugInfoExpression);
        }

        #endregion Conditional

        #region Coalesce

        private void EmitCoalesceBinaryExpression(Expression expr)
        {
            var b = (BinaryExpression)expr;
            Debug.Assert(b.Method == null);

            if (b.Left.Type.IsNullableType())
            {
                EmitNullableCoalesce(b);
            }
            else if (b.Left.Type.IsValueType)
            {
                throw Error.CoalesceUsedOnNonNullType();
            }
            else if (b.Conversion != null)
            {
                EmitLambdaReferenceCoalesce(b);
            }
            else
            {
                EmitReferenceCoalesceWithoutConversion(b);
            }
        }

        private void EmitNullableCoalesce(BinaryExpression b)
        {
            Debug.Assert(b.Method == null);

            var loc = GetLocal(b.Left.Type);
            var labIfNull = _ilg.DefineLabel();
            var labEnd = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Stloc, loc);
            _ilg.Emit(OpCodes.Ldloca, loc);
            _ilg.EmitHasValue(b.Left.Type);
            _ilg.Emit(OpCodes.Brfalse, labIfNull);

            var nnLeftType = b.Left.Type.GetNonNullableType();
            if (b.Conversion != null)
            {
                Debug.Assert(b.Conversion.Parameters.Count == 1);
                var p = b.Conversion.Parameters[0];
                Debug.Assert(p.Type.IsAssignableFrom(b.Left.Type) ||
                             p.Type.IsAssignableFrom(nnLeftType));

                // emit the delegate instance
                EmitLambdaExpression(b.Conversion);

                // emit argument
                if (!p.Type.IsAssignableFrom(b.Left.Type))
                {
                    _ilg.Emit(OpCodes.Ldloca, loc);
                    _ilg.EmitGetValueOrDefault(b.Left.Type);
                }
                else
                {
                    _ilg.Emit(OpCodes.Ldloc, loc);
                }

                // emit call to invoke
                _ilg.Emit(OpCodes.Callvirt, b.Conversion.Type.GetMethod("Invoke"));
            }
            else if (b.Type != nnLeftType)
            {
                _ilg.Emit(OpCodes.Ldloca, loc);
                _ilg.EmitGetValueOrDefault(b.Left.Type);
                _ilg.EmitConvertToType(nnLeftType, b.Type, true);
            }
            else
            {
                _ilg.Emit(OpCodes.Ldloca, loc);
                _ilg.EmitGetValueOrDefault(b.Left.Type);
            }
            FreeLocal(loc);

            _ilg.Emit(OpCodes.Br, labEnd);
            _ilg.MarkLabel(labIfNull);
            EmitExpression(b.Right);
            if (b.Right.Type != b.Type)
            {
                _ilg.EmitConvertToType(b.Right.Type, b.Type, true);
            }
            _ilg.MarkLabel(labEnd);
        }

        private void EmitLambdaReferenceCoalesce(BinaryExpression b)
        {
            var loc = GetLocal(b.Left.Type);
            var labEnd = _ilg.DefineLabel();
            var labNotNull = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Dup);
            _ilg.Emit(OpCodes.Stloc, loc);
            _ilg.Emit(OpCodes.Ldnull);
            _ilg.Emit(OpCodes.Ceq);
            _ilg.Emit(OpCodes.Brfalse, labNotNull);
            EmitExpression(b.Right);
            _ilg.Emit(OpCodes.Br, labEnd);

            // if not null, call conversion
            _ilg.MarkLabel(labNotNull);
            Debug.Assert(b.Conversion.Parameters.Count == 1);

            // emit the delegate instance
            EmitLambdaExpression(b.Conversion);

            // emit argument
            _ilg.Emit(OpCodes.Ldloc, loc);
            FreeLocal(loc);

            // emit call to invoke
            _ilg.Emit(OpCodes.Callvirt, b.Conversion.Type.GetMethod("Invoke"));

            _ilg.MarkLabel(labEnd);
        }

        private void EmitReferenceCoalesceWithoutConversion(BinaryExpression b)
        {
            var labEnd = _ilg.DefineLabel();
            var labCast = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Dup);
            _ilg.Emit(OpCodes.Ldnull);
            _ilg.Emit(OpCodes.Ceq);
            _ilg.Emit(OpCodes.Brfalse, labCast);
            _ilg.Emit(OpCodes.Pop);
            EmitExpression(b.Right);
            if (b.Right.Type != b.Type)
            {
                if (b.Right.Type.IsValueType)
                {
                    _ilg.Emit(OpCodes.Box, b.Right.Type);
                }
                _ilg.Emit(OpCodes.Castclass, b.Type);
            }
            _ilg.Emit(OpCodes.Br_S, labEnd);
            _ilg.MarkLabel(labCast);
            if (b.Left.Type != b.Type)
            {
                Debug.Assert(!b.Left.Type.IsValueType);
                _ilg.Emit(OpCodes.Castclass, b.Type);
            }
            _ilg.MarkLabel(labEnd);
        }

        #endregion Coalesce

        #region AndAlso

        private void EmitLiftedAndAlso(BinaryExpression b)
        {
            var type = typeof(bool?);
            var labComputeRight = _ilg.DefineLabel();
            var labReturnFalse = _ilg.DefineLabel();
            var labReturnNull = _ilg.DefineLabel();
            var labReturnValue = _ilg.DefineLabel();
            var labExit = _ilg.DefineLabel();
            var locLeft = GetLocal(type);
            var locRight = GetLocal(type);
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Stloc, locLeft);
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse, labComputeRight);
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitGetValueOrDefault(type);
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.Emit(OpCodes.Ceq);
            _ilg.Emit(OpCodes.Brtrue, labReturnFalse);
            // compute right
            _ilg.MarkLabel(labComputeRight);
            EmitExpression(b.Right);
            _ilg.Emit(OpCodes.Stloc, locRight);
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse_S, labReturnNull);
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitGetValueOrDefault(type);
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.Emit(OpCodes.Ceq);
            _ilg.Emit(OpCodes.Brtrue_S, labReturnFalse);
            // check left for null again
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse, labReturnNull);
            // return true
            _ilg.Emit(OpCodes.Ldc_I4_1);
            _ilg.Emit(OpCodes.Br_S, labReturnValue);
            // return false
            _ilg.MarkLabel(labReturnFalse);
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.Emit(OpCodes.Br_S, labReturnValue);
            _ilg.MarkLabel(labReturnValue);
            var ci = type.GetConstructor(new[] { typeof(bool) });
            _ilg.Emit(OpCodes.Newobj, ci);
            _ilg.Emit(OpCodes.Stloc, locLeft);
            _ilg.Emit(OpCodes.Br, labExit);
            // return null
            _ilg.MarkLabel(labReturnNull);
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.Emit(OpCodes.Initobj, type);
            _ilg.MarkLabel(labExit);
            _ilg.Emit(OpCodes.Ldloc, locLeft);
            FreeLocal(locLeft);
            FreeLocal(locRight);
        }

        private void EmitMethodAndAlso(BinaryExpression b, CompilationFlags flags)
        {
            var labEnd = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Dup);
            var opFalse = TypeHelper.GetBooleanOperator(b.Method.DeclaringType, "op_False");
            Debug.Assert(opFalse != null, "factory should check that the method exists");
            _ilg.Emit(OpCodes.Call, opFalse);
            _ilg.Emit(OpCodes.Brtrue, labEnd);

            //store the value of the left value before emitting b.Right to empty the evaluation stack
            var locLeft = GetLocal(b.Left.Type);
            _ilg.Emit(OpCodes.Stloc, locLeft);

            EmitExpression(b.Right);
            //store the right value to local
            var locRight = GetLocal(b.Right.Type);
            _ilg.Emit(OpCodes.Stloc, locRight);

            Debug.Assert(b.Method.IsStatic);
            _ilg.Emit(OpCodes.Ldloc, locLeft);
            _ilg.Emit(OpCodes.Ldloc, locRight);
            if ((flags & CompilationFlags.EmitAsTailCallMask) == CompilationFlags.EmitAsTail)
            {
                _ilg.Emit(OpCodes.Tailcall);
            }
            _ilg.Emit(OpCodes.Call, b.Method);
            FreeLocal(locLeft);
            FreeLocal(locRight);
            _ilg.MarkLabel(labEnd);
        }

        private void EmitUnliftedAndAlso(BinaryExpression b)
        {
            var @else = _ilg.DefineLabel();
            var end = _ilg.DefineLabel();
            EmitExpressionAndBranch(false, b.Left, @else);
            EmitExpression(b.Right);
            _ilg.Emit(OpCodes.Br, end);
            _ilg.MarkLabel(@else);
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.MarkLabel(end);
        }

        private void EmitAndAlsoBinaryExpression(Expression expr, CompilationFlags flags)
        {
            var b = (BinaryExpression)expr;

            if (b.Method != null && !b.IsLiftedLogical)
            {
                EmitMethodAndAlso(b, flags);
            }
            else if (b.Left.Type == typeof(bool?))
            {
                EmitLiftedAndAlso(b);
            }
            else if (b.IsLiftedLogical)
            {
                EmitExpression(b.ReduceUserdefinedLifted());
            }
            else
            {
                EmitUnliftedAndAlso(b);
            }
        }

        #endregion AndAlso

        #region OrElse

        private void EmitLiftedOrElse(BinaryExpression b)
        {
            var type = typeof(bool?);
            var labComputeRight = _ilg.DefineLabel();
            var labReturnTrue = _ilg.DefineLabel();
            var labReturnNull = _ilg.DefineLabel();
            var labReturnValue = _ilg.DefineLabel();
            var labExit = _ilg.DefineLabel();
            var locLeft = GetLocal(type);
            var locRight = GetLocal(type);
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Stloc, locLeft);
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse, labComputeRight);
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitGetValueOrDefault(type);
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.Emit(OpCodes.Ceq);
            _ilg.Emit(OpCodes.Brfalse, labReturnTrue);
            // compute right
            _ilg.MarkLabel(labComputeRight);
            EmitExpression(b.Right);
            _ilg.Emit(OpCodes.Stloc, locRight);
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse_S, labReturnNull);
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitGetValueOrDefault(type);
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.Emit(OpCodes.Ceq);
            _ilg.Emit(OpCodes.Brfalse_S, labReturnTrue);
            // check left for null again
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse, labReturnNull);
            // return false
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.Emit(OpCodes.Br_S, labReturnValue);
            // return true
            _ilg.MarkLabel(labReturnTrue);
            _ilg.Emit(OpCodes.Ldc_I4_1);
            _ilg.Emit(OpCodes.Br_S, labReturnValue);
            _ilg.MarkLabel(labReturnValue);
            var ci = type.GetConstructor(new[] { typeof(bool) });
            _ilg.Emit(OpCodes.Newobj, ci);
            _ilg.Emit(OpCodes.Stloc, locLeft);
            _ilg.Emit(OpCodes.Br, labExit);
            // return null
            _ilg.MarkLabel(labReturnNull);
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.Emit(OpCodes.Initobj, type);
            _ilg.MarkLabel(labExit);
            _ilg.Emit(OpCodes.Ldloc, locLeft);
            FreeLocal(locLeft);
            FreeLocal(locRight);
        }

        private void EmitUnliftedOrElse(BinaryExpression b)
        {
            var @else = _ilg.DefineLabel();
            var end = _ilg.DefineLabel();
            EmitExpressionAndBranch(false, b.Left, @else);
            _ilg.Emit(OpCodes.Ldc_I4_1);
            _ilg.Emit(OpCodes.Br, end);
            _ilg.MarkLabel(@else);
            EmitExpression(b.Right);
            _ilg.MarkLabel(end);
        }

        private void EmitMethodOrElse(BinaryExpression b, CompilationFlags flags)
        {
            var labEnd = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Dup);
            var opTrue = TypeHelper.GetBooleanOperator(b.Method.DeclaringType, "op_True");
            Debug.Assert(opTrue != null, "factory should check that the method exists");
            _ilg.Emit(OpCodes.Call, opTrue);
            _ilg.Emit(OpCodes.Brtrue, labEnd);

            //store the value of the left value before emitting b.Right to empty the evaluation stack
            var locLeft = GetLocal(b.Left.Type);
            _ilg.Emit(OpCodes.Stloc, locLeft);

            EmitExpression(b.Right);
            //store the right value to local
            var locRight = GetLocal(b.Right.Type);
            _ilg.Emit(OpCodes.Stloc, locRight);

            Debug.Assert(b.Method.IsStatic);
            _ilg.Emit(OpCodes.Ldloc, locLeft);
            _ilg.Emit(OpCodes.Ldloc, locRight);
            if ((flags & CompilationFlags.EmitAsTailCallMask) == CompilationFlags.EmitAsTail)
            {
                _ilg.Emit(OpCodes.Tailcall);
            }
            _ilg.Emit(OpCodes.Call, b.Method);
            FreeLocal(locLeft);
            FreeLocal(locRight);
            _ilg.MarkLabel(labEnd);
        }

        private void EmitOrElseBinaryExpression(Expression expr, CompilationFlags flags)
        {
            var b = (BinaryExpression)expr;

            if (b.Method != null && !b.IsLiftedLogical)
            {
                EmitMethodOrElse(b, flags);
            }
            else if (b.Left.Type == typeof(bool?))
            {
                EmitLiftedOrElse(b);
            }
            else if (b.IsLiftedLogical)
            {
                EmitExpression(b.ReduceUserdefinedLifted());
            }
            else
            {
                EmitUnliftedOrElse(b);
            }
        }

        #endregion OrElse

        #region Optimized branching

        /// <summary>
        /// Emits the expression and then either brtrue/brfalse to the label.
        /// </summary>
        /// <param name="branchValue">True for brtrue, false for brfalse.</param>
        /// <param name="node">The expression to emit.</param>
        /// <param name="label">The label to conditionally branch to.</param>
        /// <remarks>
        /// This function optimizes equality and short circuiting logical
        /// operators to avoid double-branching, minimize instruction count,
        /// and generate similar IL to the C# compiler. This is important for
        /// the JIT to optimize patterns like:
        ///     x != null AndAlso x.GetType() == typeof(SomeType)
        ///
        /// One optimization we don't do: we always emits at least one
        /// conditional branch to the label, and always possibly falls through,
        /// even if we know if the branch will always succeed or always fail.
        /// We do this to avoid generating unreachable code, which is fine for
        /// the CLR JIT, but doesn't verify with peverify.
        ///
        /// This kind of optimization could be implemented safely, by doing
        /// constant folding over conditionals and logical expressions at the
        /// tree level.
        /// </remarks>
        private void EmitExpressionAndBranch(bool branchValue, Expression node, Label label)
        {
            var startEmitted = EmitExpressionStart(node);
            try
            {
                if (node.Type == typeof(bool))
                {
                    switch (node.NodeType)
                    {
                        case ExpressionType.Not:
                            EmitBranchNot(branchValue, (UnaryExpression)node, label);
                            return;

                        case ExpressionType.AndAlso:
                        case ExpressionType.OrElse:
                            EmitBranchLogical(branchValue, (BinaryExpression)node, label);
                            return;

                        case ExpressionType.Block:
                            EmitBranchBlock(branchValue, (BlockExpression)node, label);
                            return;

                        case ExpressionType.Equal:
                        case ExpressionType.NotEqual:
                            EmitBranchComparison(branchValue, (BinaryExpression)node, label);
                            return;
                    }
                }
                EmitExpression(node, CompilationFlags.EmitAsNoTail | CompilationFlags.EmitNoExpressionStart);
                EmitBranchOp(branchValue, label);
            }
            finally
            {
                EmitExpressionEnd(startEmitted);
            }
        }

        private void EmitBranchOp(bool branch, Label label)
        {
            _ilg.Emit(branch ? OpCodes.Brtrue : OpCodes.Brfalse, label);
        }

        private void EmitBranchNot(bool branch, UnaryExpression node, Label label)
        {
            if (node.Method != null)
            {
                EmitExpression(node, CompilationFlags.EmitAsNoTail | CompilationFlags.EmitNoExpressionStart);
                EmitBranchOp(branch, label);
                return;
            }
            EmitExpressionAndBranch(!branch, node.Operand, label);
        }

        private void EmitBranchComparison(bool branch, BinaryExpression node, Label label)
        {
            Debug.Assert(node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual);
            Debug.Assert(!node.IsLiftedToNull);

            // To share code paths, we want to treat NotEqual as an inverted Equal
            var branchWhenEqual = branch == (node.NodeType == ExpressionType.Equal);

            if (node.Method != null)
            {
                EmitBinaryMethod(node, CompilationFlags.EmitAsNoTail);
                // EmitBinaryMethod takes into account the Equal/NotEqual
                // node kind, so use the original branch value
                EmitBranchOp(branch, label);
            }
            else if (ConstantCheck.IsNull(node.Left))
            {
                if (node.Right.Type.IsNullableType())
                {
                    EmitAddress(node.Right, node.Right.Type);
                    _ilg.EmitHasValue(node.Right.Type);
                }
                else
                {
                    Debug.Assert(!node.Right.Type.IsValueType);
                    EmitExpression(GetEqualityOperand(node.Right));
                }
                EmitBranchOp(!branchWhenEqual, label);
            }
            else if (ConstantCheck.IsNull(node.Right))
            {
                if (node.Left.Type.IsNullableType())
                {
                    EmitAddress(node.Left, node.Left.Type);
                    _ilg.EmitHasValue(node.Left.Type);
                }
                else
                {
                    Debug.Assert(!node.Left.Type.IsValueType);
                    EmitExpression(GetEqualityOperand(node.Left));
                }
                EmitBranchOp(!branchWhenEqual, label);
            }
            else if (node.Left.Type.IsNullableType() || node.Right.Type.IsNullableType())
            {
                EmitBinaryExpression(node);
                // EmitBinaryExpression takes into account the Equal/NotEqual
                // node kind, so use the original branch value
                EmitBranchOp(branch, label);
            }
            else
            {
                EmitExpression(GetEqualityOperand(node.Left));
                EmitExpression(GetEqualityOperand(node.Right));
                if (branchWhenEqual)
                {
                    _ilg.Emit(OpCodes.Beq, label);
                }
                else
                {
                    _ilg.Emit(OpCodes.Ceq);
                    _ilg.Emit(OpCodes.Brfalse, label);
                }
            }
        }

        // For optimized Equal/NotEqual, we can eliminate reference
        // conversions. IL allows comparing managed pointers regardless of
        // type. See ECMA-335 "Binary Comparison or Branch Operations", in
        // Partition III, Section 1.5 Table 4.
        private static Expression GetEqualityOperand(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Convert)
            {
                var convert = (UnaryExpression)expression;
                if (TypeHelper.AreReferenceAssignable(convert.Type, convert.Operand.Type))
                {
                    return convert.Operand;
                }
            }
            return expression;
        }

        private void EmitBranchLogical(bool branch, BinaryExpression node, Label label)
        {
            Debug.Assert(node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse);
            Debug.Assert(!node.IsLiftedToNull);

            if (node.Method != null || node.IsLifted)
            {
                EmitExpression(node);
                EmitBranchOp(branch, label);
                return;
            }

            var isAnd = node.NodeType == ExpressionType.AndAlso;

            // To share code, we make the following substitutions:
            //     if (!(left || right)) branch value
            // becomes:
            //     if (!left && !right) branch value
            // and:
            //     if (!(left && right)) branch value
            // becomes:
            //     if (!left || !right) branch value
            //
            // The observation is that "brtrue(x && y)" has the same codegen as
            // "brfalse(x || y)" except the branches have the opposite sign.
            // Same for "brfalse(x && y)" and "brtrue(x || y)".
            //
            if (branch == isAnd)
            {
                EmitBranchAnd(branch, node, label);
            }
            else
            {
                EmitBranchOr(branch, node, label);
            }
        }

        // Generates optimized AndAlso with branch == true
        // or optimized OrElse with branch == false
        private void EmitBranchAnd(bool branch, BinaryExpression node, Label label)
        {
            // if (left) then
            //   if (right) branch label
            // endif

            var endif = _ilg.DefineLabel();
            EmitExpressionAndBranch(!branch, node.Left, endif);
            EmitExpressionAndBranch(branch, node.Right, label);
            _ilg.MarkLabel(endif);
        }

        // Generates optimized OrElse with branch == true
        // or optimized AndAlso with branch == false
        private void EmitBranchOr(bool branch, BinaryExpression node, Label label)
        {
            // if (left OR right) branch label

            EmitExpressionAndBranch(branch, node.Left, label);
            EmitExpressionAndBranch(branch, node.Right, label);
        }

        private void EmitBranchBlock(bool branch, BlockExpression node, Label label)
        {
            EnterScope(node);

            var count = node.ExpressionCount;
            for (var i = 0; i < count - 1; i++)
            {
                EmitExpressionAsVoid(node.GetExpression(i));
            }
            EmitExpressionAndBranch(branch, node.GetExpression(count - 1), label);

            ExitScope(node);
        }

        #endregion Optimized branching
    }
}

#endif