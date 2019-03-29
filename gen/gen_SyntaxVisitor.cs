public abstract class SyntaxVisitor
{
	public virtual void DefaultVisit(SyntaxNode value)
	{
	}

	// 名称语法
	public virtual void VisitNameSyntax(NameSyntax value)
	{
		DefaultVisit(value);
	}

	// 简单名称
	public virtual void VisitSimpleNameSyntax(SimpleNameSyntax value)
	{
		DefaultVisit(value);
	}

	// 标识符名称
	public virtual void VisitIdentifierNameSyntax(IdentifierNameSyntax value)
	{
		DefaultVisit(value);
	}

	// 组合名称
	public virtual void VisitQualifiedNameSyntax(QualifiedNameSyntax value)
	{
		DefaultVisit(value);
	}

	// 泛型名称
	public virtual void VisitGenericNameSyntax(GenericNameSyntax value)
	{
		DefaultVisit(value);
	}

	// 泛型参数
	public virtual void VisitTypeArgumentListSyntax(TypeArgumentListSyntax value)
	{
		DefaultVisit(value);
	}

	// 类型语法
	public virtual void VisitTypeSyntax(TypeSyntax value)
	{
		DefaultVisit(value);
	}

	// 预定义类型
	public virtual void VisitPredefinedTypeSyntax(PredefinedTypeSyntax value)
	{
		DefaultVisit(value);
	}

	// 数组类型
	public virtual void VisitArrayTypeSyntax(ArrayTypeSyntax value)
	{
		DefaultVisit(value);
	}

	// 数组维度
	public virtual void VisitArrayRankSpecifierSyntax(ArrayRankSpecifierSyntax value)
	{
		DefaultVisit(value);
	}

	// 指针类型
	public virtual void VisitPointerTypeSyntax(PointerTypeSyntax value)
	{
		DefaultVisit(value);
	}

	// 引用类型
	public virtual void VisitRefTypeSyntax(RefTypeSyntax value)
	{
		DefaultVisit(value);
	}

	// 表达式语法
	public virtual void VisitExpressionSyntax(ExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 括号表达式
	public virtual void VisitParenthesizedExpressionSyntax(ParenthesizedExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 前缀一元表达式
	public virtual void VisitPrefixUnaryExpressionSyntax(PrefixUnaryExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 后缀一元表达式
	public virtual void VisitPostfixUnaryExpressionSyntax(PostfixUnaryExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 成员访问表达式
	public virtual void VisitMemberAccessExpressionSyntax(MemberAccessExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 二元算数表达式
	public virtual void VisitBinaryExpressionSyntax(BinaryExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 赋值表达式
	public virtual void VisitAssignmentExpressionSyntax(AssignmentExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 条件表达式
	public virtual void VisitConditionalExpressionSyntax(ConditionalExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 实例表达式
	public virtual void VisitInstanceExpressionSyntax(InstanceExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// this表达式
	public virtual void VisitThisExpressionSyntax(ThisExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// base表达式
	public virtual void VisitBaseExpressionSyntax(BaseExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 字面量表达式
	public virtual void VisitLiteralExpressionSyntax(LiteralExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// default表达式
	public virtual void VisitDefaultExpressionSyntax(DefaultExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// typeof表达式
	public virtual void VisitTypeOfExpressionSyntax(TypeOfExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// sizeof表达式
	public virtual void VisitSizeOfExpressionSyntax(SizeOfExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 函数调用表达式
	public virtual void VisitInvocationExpressionSyntax(InvocationExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 元素访问表达式
	public virtual void VisitElementAccessExpressionSyntax(ElementAccessExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 参数列表基类
	public virtual void VisitBaseArgumentListSyntax(BaseArgumentListSyntax value)
	{
		DefaultVisit(value);
	}

	// 参数列表
	public virtual void VisitArgumentListSyntax(ArgumentListSyntax value)
	{
		DefaultVisit(value);
	}

	// 方括号参数列表
	public virtual void VisitBracketedArgumentListSyntax(BracketedArgumentListSyntax value)
	{
		DefaultVisit(value);
	}

	// 参数语法
	public virtual void VisitArgumentSyntax(ArgumentSyntax value)
	{
		DefaultVisit(value);
	}

	// 变量声明语法
	public virtual void VisitDeclarationExpressionSyntax(DeclarationExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 类型转换表达式
	public virtual void VisitCastExpressionSyntax(CastExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 初始化列表语法
	public virtual void VisitInitializerExpressionSyntax(InitializerExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// new对象表达式
	public virtual void VisitObjectCreationExpressionSyntax(ObjectCreationExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// new数组表达式
	public virtual void VisitArrayCreationExpressionSyntax(ArrayCreationExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// is表达式
	public virtual void VisitIsPatternExpressionSyntax(IsPatternExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// throw表达式
	public virtual void VisitThrowExpressionSyntax(ThrowExpressionSyntax value)
	{
		DefaultVisit(value);
	}

	// 全局语句
	public virtual void VisitGlobalStatementSyntax(GlobalStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// 语句语法
	public virtual void VisitStatementSyntax(StatementSyntax value)
	{
		DefaultVisit(value);
	}

	// 块语句
	public virtual void VisitBlockSyntax(BlockSyntax value)
	{
		DefaultVisit(value);
	}

	// 局部变量声明
	public virtual void VisitLocalDeclarationStatementSyntax(LocalDeclarationStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// 变量声明
	public virtual void VisitVariableDeclarationSyntax(VariableDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 变量定义
	public virtual void VisitVariableDeclaratorSyntax(VariableDeclaratorSyntax value)
	{
		DefaultVisit(value);
	}

	// 赋初值
	public virtual void VisitEqualsValueClauseSyntax(EqualsValueClauseSyntax value)
	{
		DefaultVisit(value);
	}

	// 变量指定
	public virtual void VisitVariableDesignationSyntax(VariableDesignationSyntax value)
	{
		DefaultVisit(value);
	}

	// 简单变量指定
	public virtual void VisitSingleVariableDesignationSyntax(SingleVariableDesignationSyntax value)
	{
		DefaultVisit(value);
	}

	// 表达式语句
	public virtual void VisitExpressionStatementSyntax(ExpressionStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// 空语句
	public virtual void VisitEmptyStatementSyntax(EmptyStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// 标签语句
	public virtual void VisitLabeledStatementSyntax(LabeledStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// goto语句
	public virtual void VisitGotoStatementSyntax(GotoStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// break语句
	public virtual void VisitBreakStatementSyntax(BreakStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// continue语句
	public virtual void VisitContinueStatementSyntax(ContinueStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// return语句
	public virtual void VisitReturnStatementSyntax(ReturnStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// throw语句
	public virtual void VisitThrowStatementSyntax(ThrowStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// while语句
	public virtual void VisitWhileStatementSyntax(WhileStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// do-while语句
	public virtual void VisitDoStatementSyntax(DoStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// for语句
	public virtual void VisitForStatementSyntax(ForStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// foreach语句
	public virtual void VisitForEachStatementSyntax(ForEachStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// if语句
	public virtual void VisitIfStatementSyntax(IfStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// else语句
	public virtual void VisitElseClauseSyntax(ElseClauseSyntax value)
	{
		DefaultVisit(value);
	}

	// switch语句
	public virtual void VisitSwitchStatementSyntax(SwitchStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// switch-case区段
	public virtual void VisitSwitchSectionSyntax(SwitchSectionSyntax value)
	{
		DefaultVisit(value);
	}

	// case标签
	public virtual void VisitSwitchLabelSyntax(SwitchLabelSyntax value)
	{
		DefaultVisit(value);
	}

	// try语句
	public virtual void VisitTryStatementSyntax(TryStatementSyntax value)
	{
		DefaultVisit(value);
	}

	// Catch语句
	public virtual void VisitCatchClauseSyntax(CatchClauseSyntax value)
	{
		DefaultVisit(value);
	}

	// 异常描述
	public virtual void VisitCatchDeclarationSyntax(CatchDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// Finally体
	public virtual void VisitFinallyClauseSyntax(FinallyClauseSyntax value)
	{
		DefaultVisit(value);
	}

	// 文件单元
	public virtual void VisitFileUnitSyntax(FileUnitSyntax value)
	{
		DefaultVisit(value);
	}

	// using声明
	public virtual void VisitUsingDirectiveSyntax(UsingDirectiveSyntax value)
	{
		DefaultVisit(value);
	}

	// 成员定义
	public virtual void VisitMemberDeclarationSyntax(MemberDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 命名空间定义
	public virtual void VisitNamespaceDeclarationSyntax(NamespaceDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 属性定义
	public virtual void VisitAttributeListSyntax(AttributeListSyntax value)
	{
		DefaultVisit(value);
	}

	// 名称=
	public virtual void VisitNameEqualsSyntax(NameEqualsSyntax value)
	{
		DefaultVisit(value);
	}

	// 类型形参列表
	public virtual void VisitTypeParameterListSyntax(TypeParameterListSyntax value)
	{
		DefaultVisit(value);
	}

	// 类型形参
	public virtual void VisitTypeParameterSyntax(TypeParameterSyntax value)
	{
		DefaultVisit(value);
	}

	// 类型定义基类
	public virtual void VisitBaseTypeDeclarationSyntax(BaseTypeDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 类型定义
	public virtual void VisitTypeDeclarationSyntax(TypeDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 类定义
	public virtual void VisitClassDeclarationSyntax(ClassDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 结构体定义
	public virtual void VisitStructDeclarationSyntax(StructDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 接口定义
	public virtual void VisitInterfaceDeclarationSyntax(InterfaceDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 枚举类型
	public virtual void VisitEnumDeclarationSyntax(EnumDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 委托类型
	public virtual void VisitDelegateDeclarationSyntax(DelegateDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 枚举成员
	public virtual void VisitEnumMemberDeclarationSyntax(EnumMemberDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 基类列表
	public virtual void VisitBaseListSyntax(BaseListSyntax value)
	{
		DefaultVisit(value);
	}

	// 字段定义
	public virtual void VisitFieldDeclarationSyntax(FieldDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 显示接口指定
	public virtual void VisitExplicitInterfaceSpecifierSyntax(ExplicitInterfaceSpecifierSyntax value)
	{
		DefaultVisit(value);
	}

	// 方法定义基类
	public virtual void VisitBaseMethodDeclarationSyntax(BaseMethodDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 方法定义
	public virtual void VisitMethodDeclarationSyntax(MethodDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 操作符方法
	public virtual void VisitOperatorDeclarationSyntax(OperatorDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 类型转换操作符
	public virtual void VisitConversionOperatorDeclarationSyntax(ConversionOperatorDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 构造器
	public virtual void VisitConstructorDeclarationSyntax(ConstructorDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 构造器初始化器
	public virtual void VisitConstructorInitializerSyntax(ConstructorInitializerSyntax value)
	{
		DefaultVisit(value);
	}

	// 析构器
	public virtual void VisitDestructorDeclarationSyntax(DestructorDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 属性定义基类
	public virtual void VisitBasePropertyDeclarationSyntax(BasePropertyDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 属性定义
	public virtual void VisitPropertyDeclarationSyntax(PropertyDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 箭头表达式
	public virtual void VisitArrowExpressionClauseSyntax(ArrowExpressionClauseSyntax value)
	{
		DefaultVisit(value);
	}

	// 下标访问
	public virtual void VisitIndexerDeclarationSyntax(IndexerDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 属性体
	public virtual void VisitAccessorListSyntax(AccessorListSyntax value)
	{
		DefaultVisit(value);
	}

	// 属性方法
	public virtual void VisitAccessorDeclarationSyntax(AccessorDeclarationSyntax value)
	{
		DefaultVisit(value);
	}

	// 形参表基类
	public virtual void VisitBaseParameterListSyntax(BaseParameterListSyntax value)
	{
		DefaultVisit(value);
	}

	// 形参表
	public virtual void VisitParameterListSyntax(ParameterListSyntax value)
	{
		DefaultVisit(value);
	}

	// 方括号形参列表
	public virtual void VisitBracketedParameterListSyntax(BracketedParameterListSyntax value)
	{
		DefaultVisit(value);
	}

	// 形参
	public virtual void VisitParameterSyntax(ParameterSyntax value)
	{
		DefaultVisit(value);
	}
}
