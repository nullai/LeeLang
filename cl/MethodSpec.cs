using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class ParameterSpec
	{
		public ParameterAttribute attr;
		public TypeSpec type;
		public string name;

		public IR_Argument CodeGen(CodeGenContext ctx)
		{
			return new IR_Argument(type.IRType);
		}
	}
	public class MethodSpec : BlockSpec
	{
		public TypeSpec return_type;
		public ParameterSpec[] parameters;
		public Statement body;
		public IR_Method ir_method;
		public MethodSpec(string name, TypeSpec type, NamespaceSpec declare)
			: base(name, declare)
		{
			return_type = type;
			ir_method = new IR_Method(name, type.IRType);
			if (type.IsStruct && type != BuildinTypeSpec.Void && ir_method.ret_type == IR_DataType.Pointer)
			{
				int size = type.SizeOf;
				switch (size)
				{
					case 1:
						ir_method.ret_type = IR_DataType.I1;
						break;
					case 2:
						ir_method.ret_type = IR_DataType.I2;
						break;
					case 4:
						ir_method.ret_type = IR_DataType.I4;
						break;
					case 8:
						if (BuildinTypeSpec.PointerSize == 8)
							ir_method.ret_type = IR_DataType.I8;
						break;
				}
				if (ir_method.ret_type == IR_DataType.Pointer)
				{
					ir_method.ret_arg = new IR_Argument(IR_DataType.Pointer);
				}
			}
		}
		public override void CodeGen(CodeGenContext ctx)
		{
			ctx.method = ir_method;
			if (parameters != null)
			{
				for (int i = 0; i < parameters.Length; i++)
				{
					ir_method.AddParameter(parameters[i].CodeGen(ctx));
				}
			}
			foreach (var ps in members.Values)
			{
				for (int i = 0; i < ps.Count; i++)
					ps[i].CodeGen(ctx);
			}
			ctx.module.methods.Add(ir_method);
			if (body != null)
			{
				IR_BaseBlock block = new IR_BaseBlock(ir_method);
				ir_method.blocks.Add(block);
				ctx.block = block;
				body.CodeGen(ctx);
				ctx.block = null;
			}
			ctx.method = null;
		}
	}
}
