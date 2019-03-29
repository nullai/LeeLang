
// 名称语法
public abstract class NameSyntax : TypeSyntax
{

	public NameSyntax()
	{
	}
}

// 简单名称
public abstract class SimpleNameSyntax : NameSyntax
{
	// 名称标识符
	public SyntaxToken mIdentifier;

	public SimpleNameSyntax(SyntaxToken Identifier)
	{
		this.mIdentifier = Identifier;
	}
}

// 标识符名称
public class IdentifierNameSyntax : SimpleNameSyntax
{

	public IdentifierNameSyntax(SyntaxToken Identifier)
		: base(Identifier)
	{
	}

	public override int Count => 1;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mIdentifier;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitIdentifierNameSyntax(this);
	}
}

// 组合名称
public class QualifiedNameSyntax : NameSyntax
{
	// 左边名称
	public NameSyntax mLeft;

	// 点
	public SyntaxToken mDotToken;

	// 右边名称
	public SimpleNameSyntax mRight;

	public QualifiedNameSyntax(NameSyntax Left, SyntaxToken DotToken, SimpleNameSyntax Right)
	{
		this.mLeft = Left;
		this.mDotToken = DotToken;
		this.mRight = Right;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mLeft;
		case 1:
			return mDotToken;
		case 2:
			return mRight;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitQualifiedNameSyntax(this);
	}
}

// 泛型名称
public class GenericNameSyntax : SimpleNameSyntax
{
	// 泛型参数
	public TypeArgumentListSyntax mTypeArgumentList;

	public GenericNameSyntax(SyntaxToken Identifier, TypeArgumentListSyntax TypeArgumentList)
		: base(Identifier)
	{
		this.mTypeArgumentList = TypeArgumentList;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mIdentifier;
		case 1:
			return mTypeArgumentList;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitGenericNameSyntax(this);
	}
}

// 泛型参数
public class TypeArgumentListSyntax : SyntaxNode
{
	// 左尖括号
	public SyntaxToken mLessThanToken;

	// 参数表
	public SeparatedSyntaxList<TypeSyntax> mArguments;

	// 右尖括号
	public SyntaxToken mGreaterThanToken;

	public TypeArgumentListSyntax(SyntaxToken LessThanToken, SeparatedSyntaxList<TypeSyntax> Arguments, SyntaxToken GreaterThanToken)
	{
		this.mLessThanToken = LessThanToken;
		this.mArguments = Arguments;
		this.mGreaterThanToken = GreaterThanToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mLessThanToken;
		case 1:
			return mArguments;
		case 2:
			return mGreaterThanToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTypeArgumentListSyntax(this);
	}
}

// 类型语法
public abstract class TypeSyntax : ExpressionSyntax
{

	public TypeSyntax()
	{
	}
}

// 预定义类型
public class PredefinedTypeSyntax : TypeSyntax
{
	// 类型关键字
	public SyntaxToken mKeyword;

	public PredefinedTypeSyntax(SyntaxToken Keyword)
	{
		this.mKeyword = Keyword;
	}

	public override int Count => 1;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mKeyword;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPredefinedTypeSyntax(this);
	}
}

// 数组类型
public class ArrayTypeSyntax : TypeSyntax
{
	// 元素类型
	public TypeSyntax mElementType;

	// 数组维度表
	public SyntaxList<ArrayRankSpecifierSyntax> mRankSpecifiers;

	public ArrayTypeSyntax(TypeSyntax ElementType, SyntaxList<ArrayRankSpecifierSyntax> RankSpecifiers)
	{
		this.mElementType = ElementType;
		this.mRankSpecifiers = RankSpecifiers;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mElementType;
		case 1:
			return mRankSpecifiers;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitArrayTypeSyntax(this);
	}
}

// 数组维度
public class ArrayRankSpecifierSyntax : SyntaxNode
{
	// 开方括号
	public SyntaxToken mOpenBracketToken;

	// 数组长度表
	public SeparatedSyntaxList<ExpressionSyntax> mSizes;

	// 关方括号
	public SyntaxToken mCloseBracketToken;

	public ArrayRankSpecifierSyntax(SyntaxToken OpenBracketToken, SeparatedSyntaxList<ExpressionSyntax> Sizes, SyntaxToken CloseBracketToken)
	{
		this.mOpenBracketToken = OpenBracketToken;
		this.mSizes = Sizes;
		this.mCloseBracketToken = CloseBracketToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mOpenBracketToken;
		case 1:
			return mSizes;
		case 2:
			return mCloseBracketToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitArrayRankSpecifierSyntax(this);
	}
}

// 指针类型
public class PointerTypeSyntax : TypeSyntax
{
	// 元素类型
	public TypeSyntax mElementType;

	// const
	public SyntaxToken mConstToken;

	// 星号
	public SyntaxToken mAsteriskToken;

	public PointerTypeSyntax(TypeSyntax ElementType, SyntaxToken ConstToken, SyntaxToken AsteriskToken)
	{
		this.mElementType = ElementType;
		this.mConstToken = ConstToken;
		this.mAsteriskToken = AsteriskToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mElementType;
		case 1:
			return mConstToken;
		case 2:
			return mAsteriskToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPointerTypeSyntax(this);
	}
}

// 引用类型
public class RefTypeSyntax : TypeSyntax
{
	// 元素类型
	public TypeSyntax mElementType;

	// const
	public SyntaxToken mConstToken;

	// &符号
	public SyntaxToken mAmpersandToken;

	public RefTypeSyntax(TypeSyntax ElementType, SyntaxToken ConstToken, SyntaxToken AmpersandToken)
	{
		this.mElementType = ElementType;
		this.mConstToken = ConstToken;
		this.mAmpersandToken = AmpersandToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mElementType;
		case 1:
			return mConstToken;
		case 2:
			return mAmpersandToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitRefTypeSyntax(this);
	}
}

// 表达式语法
public abstract class ExpressionSyntax : SyntaxNode
{

	public ExpressionSyntax()
	{
	}
}

// 括号表达式
public class ParenthesizedExpressionSyntax : ExpressionSyntax
{
	// 开括号
	public SyntaxToken mOpenParenToken;

	// 子表达式
	public ExpressionSyntax mExpression;

	// 关括号
	public SyntaxToken mCloseParenToken;

	public ParenthesizedExpressionSyntax(SyntaxToken OpenParenToken, ExpressionSyntax Expression, SyntaxToken CloseParenToken)
	{
		this.mOpenParenToken = OpenParenToken;
		this.mExpression = Expression;
		this.mCloseParenToken = CloseParenToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mOpenParenToken;
		case 1:
			return mExpression;
		case 2:
			return mCloseParenToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedExpressionSyntax(this);
	}
}

// 前缀一元表达式
public class PrefixUnaryExpressionSyntax : ExpressionSyntax
{
	// 操作符
	public SyntaxToken mOperatorToken;

	// 操作数
	public ExpressionSyntax mOperand;

	public PrefixUnaryExpressionSyntax(SyntaxToken OperatorToken, ExpressionSyntax Operand)
	{
		this.mOperatorToken = OperatorToken;
		this.mOperand = Operand;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mOperatorToken;
		case 1:
			return mOperand;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPrefixUnaryExpressionSyntax(this);
	}
}

// 后缀一元表达式
public class PostfixUnaryExpressionSyntax : ExpressionSyntax
{
	// 操作数
	public ExpressionSyntax mOperand;

	// 操作符
	public SyntaxToken mOperatorToken;

	public PostfixUnaryExpressionSyntax(ExpressionSyntax Operand, SyntaxToken OperatorToken)
	{
		this.mOperand = Operand;
		this.mOperatorToken = OperatorToken;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mOperand;
		case 1:
			return mOperatorToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPostfixUnaryExpressionSyntax(this);
	}
}

// 成员访问表达式
public class MemberAccessExpressionSyntax : ExpressionSyntax
{
	// 左表达式
	public ExpressionSyntax mExpression;

	// 访问符号
	public SyntaxToken mOperatorToken;

	// 成员名
	public SimpleNameSyntax mName;

	public MemberAccessExpressionSyntax(ExpressionSyntax Expression, SyntaxToken OperatorToken, SimpleNameSyntax Name)
	{
		this.mExpression = Expression;
		this.mOperatorToken = OperatorToken;
		this.mName = Name;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mExpression;
		case 1:
			return mOperatorToken;
		case 2:
			return mName;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitMemberAccessExpressionSyntax(this);
	}
}

// 二元算数表达式
public class BinaryExpressionSyntax : ExpressionSyntax
{
	// 左表达式
	public ExpressionSyntax mLeft;

	// 操作符
	public SyntaxToken mOperatorToken;

	// 右表达式
	public ExpressionSyntax mRight;

	public BinaryExpressionSyntax(ExpressionSyntax Left, SyntaxToken OperatorToken, ExpressionSyntax Right)
	{
		this.mLeft = Left;
		this.mOperatorToken = OperatorToken;
		this.mRight = Right;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mLeft;
		case 1:
			return mOperatorToken;
		case 2:
			return mRight;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBinaryExpressionSyntax(this);
	}
}

// 赋值表达式
public class AssignmentExpressionSyntax : ExpressionSyntax
{
	// 左表达式
	public ExpressionSyntax mLeft;

	// 操作符
	public SyntaxToken mOperatorToken;

	// 右表达式
	public ExpressionSyntax mRight;

	public AssignmentExpressionSyntax(ExpressionSyntax Left, SyntaxToken OperatorToken, ExpressionSyntax Right)
	{
		this.mLeft = Left;
		this.mOperatorToken = OperatorToken;
		this.mRight = Right;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mLeft;
		case 1:
			return mOperatorToken;
		case 2:
			return mRight;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitAssignmentExpressionSyntax(this);
	}
}

// 条件表达式
public class ConditionalExpressionSyntax : ExpressionSyntax
{
	// 判断表达式
	public ExpressionSyntax mCondition;

	// 问号
	public SyntaxToken mQuestionToken;

	// 真表达式
	public ExpressionSyntax mWhenTrue;

	// 冒号
	public SyntaxToken mColonToken;

	// 假表达式
	public ExpressionSyntax mWhenFalse;

	public ConditionalExpressionSyntax(ExpressionSyntax Condition, SyntaxToken QuestionToken, ExpressionSyntax WhenTrue, SyntaxToken ColonToken, ExpressionSyntax WhenFalse)
	{
		this.mCondition = Condition;
		this.mQuestionToken = QuestionToken;
		this.mWhenTrue = WhenTrue;
		this.mColonToken = ColonToken;
		this.mWhenFalse = WhenFalse;
	}

	public override int Count => 5;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mCondition;
		case 1:
			return mQuestionToken;
		case 2:
			return mWhenTrue;
		case 3:
			return mColonToken;
		case 4:
			return mWhenFalse;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitConditionalExpressionSyntax(this);
	}
}

// 实例表达式
public abstract class InstanceExpressionSyntax : ExpressionSyntax
{

	public InstanceExpressionSyntax()
	{
	}
}

// this表达式
public class ThisExpressionSyntax : InstanceExpressionSyntax
{
	// SyntaxToken representing the this keyword.
	public SyntaxToken mToken;

	public ThisExpressionSyntax(SyntaxToken Token)
	{
		this.mToken = Token;
	}

	public override int Count => 1;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitThisExpressionSyntax(this);
	}
}

// base表达式
public class BaseExpressionSyntax : InstanceExpressionSyntax
{
	// SyntaxToken representing the base keyword.
	public SyntaxToken mToken;

	public BaseExpressionSyntax(SyntaxToken Token)
	{
		this.mToken = Token;
	}

	public override int Count => 1;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBaseExpressionSyntax(this);
	}
}

// 字面量表达式
public class LiteralExpressionSyntax : ExpressionSyntax
{
	// 字面量
	public SyntaxToken mToken;

	public LiteralExpressionSyntax(SyntaxToken Token)
	{
		this.mToken = Token;
	}

	public override int Count => 1;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitLiteralExpressionSyntax(this);
	}
}

// default表达式
public class DefaultExpressionSyntax : ExpressionSyntax
{
	// default
	public SyntaxToken mKeyword;

	// 开括号
	public SyntaxToken mOpenParenToken;

	// 类型
	public TypeSyntax mType;

	// 关括号
	public SyntaxToken mCloseParenToken;

	public DefaultExpressionSyntax(SyntaxToken Keyword, SyntaxToken OpenParenToken, TypeSyntax Type, SyntaxToken CloseParenToken)
	{
		this.mKeyword = Keyword;
		this.mOpenParenToken = OpenParenToken;
		this.mType = Type;
		this.mCloseParenToken = CloseParenToken;
	}

	public override int Count => 4;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mKeyword;
		case 1:
			return mOpenParenToken;
		case 2:
			return mType;
		case 3:
			return mCloseParenToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDefaultExpressionSyntax(this);
	}
}

// typeof表达式
public class TypeOfExpressionSyntax : ExpressionSyntax
{
	// typeof
	public SyntaxToken mKeyword;

	// 开括号
	public SyntaxToken mOpenParenToken;

	// 类型
	public TypeSyntax mType;

	// 关括号
	public SyntaxToken mCloseParenToken;

	public TypeOfExpressionSyntax(SyntaxToken Keyword, SyntaxToken OpenParenToken, TypeSyntax Type, SyntaxToken CloseParenToken)
	{
		this.mKeyword = Keyword;
		this.mOpenParenToken = OpenParenToken;
		this.mType = Type;
		this.mCloseParenToken = CloseParenToken;
	}

	public override int Count => 4;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mKeyword;
		case 1:
			return mOpenParenToken;
		case 2:
			return mType;
		case 3:
			return mCloseParenToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTypeOfExpressionSyntax(this);
	}
}

// sizeof表达式
public class SizeOfExpressionSyntax : ExpressionSyntax
{
	// sizeof
	public SyntaxToken mKeyword;

	// 开括号
	public SyntaxToken mOpenParenToken;

	// 类型
	public TypeSyntax mType;

	// 关括号
	public SyntaxToken mCloseParenToken;

	public SizeOfExpressionSyntax(SyntaxToken Keyword, SyntaxToken OpenParenToken, TypeSyntax Type, SyntaxToken CloseParenToken)
	{
		this.mKeyword = Keyword;
		this.mOpenParenToken = OpenParenToken;
		this.mType = Type;
		this.mCloseParenToken = CloseParenToken;
	}

	public override int Count => 4;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mKeyword;
		case 1:
			return mOpenParenToken;
		case 2:
			return mType;
		case 3:
			return mCloseParenToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitSizeOfExpressionSyntax(this);
	}
}

// 函数调用表达式
public class InvocationExpressionSyntax : ExpressionSyntax
{
	// 目标表达式
	public ExpressionSyntax mExpression;

	// 参数表
	public ArgumentListSyntax mArgumentList;

	public InvocationExpressionSyntax(ExpressionSyntax Expression, ArgumentListSyntax ArgumentList)
	{
		this.mExpression = Expression;
		this.mArgumentList = ArgumentList;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mExpression;
		case 1:
			return mArgumentList;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitInvocationExpressionSyntax(this);
	}
}

// 元素访问表达式
public class ElementAccessExpressionSyntax : ExpressionSyntax
{
	// 目标表达式
	public ExpressionSyntax mExpression;

	// 参数表
	public BracketedArgumentListSyntax mArgumentList;

	public ElementAccessExpressionSyntax(ExpressionSyntax Expression, BracketedArgumentListSyntax ArgumentList)
	{
		this.mExpression = Expression;
		this.mArgumentList = ArgumentList;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mExpression;
		case 1:
			return mArgumentList;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitElementAccessExpressionSyntax(this);
	}
}

// 参数列表基类
public abstract class BaseArgumentListSyntax : SyntaxNode
{
	// 参数表
	public SeparatedSyntaxList<ArgumentSyntax> mArguments;

	public BaseArgumentListSyntax(SeparatedSyntaxList<ArgumentSyntax> Arguments)
	{
		this.mArguments = Arguments;
	}
}

// 参数列表
public class ArgumentListSyntax : BaseArgumentListSyntax
{
	// 开括号
	public SyntaxToken mOpenParenToken;

	// 关括号
	public SyntaxToken mCloseParenToken;

	public ArgumentListSyntax(SeparatedSyntaxList<ArgumentSyntax> Arguments, SyntaxToken OpenParenToken, SyntaxToken CloseParenToken)
		: base(Arguments)
	{
		this.mOpenParenToken = OpenParenToken;
		this.mCloseParenToken = CloseParenToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mArguments;
		case 1:
			return mOpenParenToken;
		case 2:
			return mCloseParenToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitArgumentListSyntax(this);
	}
}

// 方括号参数列表
public class BracketedArgumentListSyntax : BaseArgumentListSyntax
{
	// 开方括号
	public SyntaxToken mOpenBracketToken;

	// 关方括号
	public SyntaxToken mCloseBracketToken;

	public BracketedArgumentListSyntax(SeparatedSyntaxList<ArgumentSyntax> Arguments, SyntaxToken OpenBracketToken, SyntaxToken CloseBracketToken)
		: base(Arguments)
	{
		this.mOpenBracketToken = OpenBracketToken;
		this.mCloseBracketToken = CloseBracketToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mArguments;
		case 1:
			return mOpenBracketToken;
		case 2:
			return mCloseBracketToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBracketedArgumentListSyntax(this);
	}
}

// 参数语法
public class ArgumentSyntax : SyntaxNode
{
	// 表达式
	public ExpressionSyntax mExpression;

	public ArgumentSyntax(ExpressionSyntax Expression)
	{
		this.mExpression = Expression;
	}

	public override int Count => 1;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mExpression;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitArgumentSyntax(this);
	}
}

// 变量声明语法
public class DeclarationExpressionSyntax : ExpressionSyntax
{
	// 变量类型
	public TypeSyntax mType;

	// 变量列表
	public VariableDesignationSyntax mDesignation;

	public DeclarationExpressionSyntax(TypeSyntax Type, VariableDesignationSyntax Designation)
	{
		this.mType = Type;
		this.mDesignation = Designation;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mType;
		case 1:
			return mDesignation;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDeclarationExpressionSyntax(this);
	}
}

// 类型转换表达式
public class CastExpressionSyntax : ExpressionSyntax
{
	// 开括号
	public SyntaxToken mOpenParenToken;

	// 类型
	public TypeSyntax mType;

	// 关括号
	public SyntaxToken mCloseParenToken;

	// 表达式
	public ExpressionSyntax mExpression;

	public CastExpressionSyntax(SyntaxToken OpenParenToken, TypeSyntax Type, SyntaxToken CloseParenToken, ExpressionSyntax Expression)
	{
		this.mOpenParenToken = OpenParenToken;
		this.mType = Type;
		this.mCloseParenToken = CloseParenToken;
		this.mExpression = Expression;
	}

	public override int Count => 4;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mOpenParenToken;
		case 1:
			return mType;
		case 2:
			return mCloseParenToken;
		case 3:
			return mExpression;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitCastExpressionSyntax(this);
	}
}

// 初始化列表语法
public class InitializerExpressionSyntax : ExpressionSyntax
{
	// 开大括号
	public SyntaxToken mOpenBraceToken;

	// 表达式列表
	public SeparatedSyntaxList<ExpressionSyntax> mExpressions;

	// 关大括号
	public SyntaxToken mCloseBraceToken;

	public InitializerExpressionSyntax(SyntaxToken OpenBraceToken, SeparatedSyntaxList<ExpressionSyntax> Expressions, SyntaxToken CloseBraceToken)
	{
		this.mOpenBraceToken = OpenBraceToken;
		this.mExpressions = Expressions;
		this.mCloseBraceToken = CloseBraceToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mOpenBraceToken;
		case 1:
			return mExpressions;
		case 2:
			return mCloseBraceToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitInitializerExpressionSyntax(this);
	}
}

// new对象表达式
public class ObjectCreationExpressionSyntax : ExpressionSyntax
{
	// new
	public SyntaxToken mNewKeyword;

	// 类型
	public TypeSyntax mType;

	// 参数表
	public ArgumentListSyntax mArgumentList;

	// 初始化列表
	public InitializerExpressionSyntax mInitializer;

	public ObjectCreationExpressionSyntax(SyntaxToken NewKeyword, TypeSyntax Type, ArgumentListSyntax ArgumentList, InitializerExpressionSyntax Initializer)
	{
		this.mNewKeyword = NewKeyword;
		this.mType = Type;
		this.mArgumentList = ArgumentList;
		this.mInitializer = Initializer;
	}

	public override int Count => 4;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mNewKeyword;
		case 1:
			return mType;
		case 2:
			return mArgumentList;
		case 3:
			return mInitializer;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitObjectCreationExpressionSyntax(this);
	}
}

// new数组表达式
public class ArrayCreationExpressionSyntax : ExpressionSyntax
{
	// new
	public SyntaxToken mNewKeyword;

	// 类型
	public ArrayTypeSyntax mType;

	// 初始化列表
	public InitializerExpressionSyntax mInitializer;

	public ArrayCreationExpressionSyntax(SyntaxToken NewKeyword, ArrayTypeSyntax Type, InitializerExpressionSyntax Initializer)
	{
		this.mNewKeyword = NewKeyword;
		this.mType = Type;
		this.mInitializer = Initializer;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mNewKeyword;
		case 1:
			return mType;
		case 2:
			return mInitializer;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitArrayCreationExpressionSyntax(this);
	}
}

// is表达式
public class IsPatternExpressionSyntax : ExpressionSyntax
{
	// 表达式
	public ExpressionSyntax mExpression;

	// is
	public SyntaxToken mIsKeyword;

	// 类型
	public TypeSyntax mPattern;

	public IsPatternExpressionSyntax(ExpressionSyntax Expression, SyntaxToken IsKeyword, TypeSyntax Pattern)
	{
		this.mExpression = Expression;
		this.mIsKeyword = IsKeyword;
		this.mPattern = Pattern;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mExpression;
		case 1:
			return mIsKeyword;
		case 2:
			return mPattern;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitIsPatternExpressionSyntax(this);
	}
}

// throw表达式
public class ThrowExpressionSyntax : ExpressionSyntax
{
	// throw
	public SyntaxToken mThrowKeyword;

	// 表达式
	public ExpressionSyntax mExpression;

	public ThrowExpressionSyntax(SyntaxToken ThrowKeyword, ExpressionSyntax Expression)
	{
		this.mThrowKeyword = ThrowKeyword;
		this.mExpression = Expression;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mThrowKeyword;
		case 1:
			return mExpression;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitThrowExpressionSyntax(this);
	}
}

// 全局语句
public class GlobalStatementSyntax : MemberDeclarationSyntax
{
	// 语句
	public StatementSyntax mStatement;

	public GlobalStatementSyntax(StatementSyntax Statement)
	{
		this.mStatement = Statement;
	}

	public override int Count => 1;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mStatement;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitGlobalStatementSyntax(this);
	}
}

// 语句语法
public abstract class StatementSyntax : SyntaxNode
{

	public StatementSyntax()
	{
	}
}

// 块语句
public class BlockSyntax : StatementSyntax
{
	// 开大括号
	public SyntaxToken mOpenBraceToken;

	// 子语句
	public SyntaxList<StatementSyntax> mStatements;

	// 关大括号
	public SyntaxToken mCloseBraceToken;

	public BlockSyntax(SyntaxToken OpenBraceToken, SyntaxList<StatementSyntax> Statements, SyntaxToken CloseBraceToken)
	{
		this.mOpenBraceToken = OpenBraceToken;
		this.mStatements = Statements;
		this.mCloseBraceToken = CloseBraceToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mOpenBraceToken;
		case 1:
			return mStatements;
		case 2:
			return mCloseBraceToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBlockSyntax(this);
	}
}

// 局部变量声明
public class LocalDeclarationStatementSyntax : StatementSyntax
{
	// 修饰列表
	public SyntaxList<SyntaxToken> mModifiers;

	// 变量声明
	public VariableDeclarationSyntax mDeclaration;

	// 分号
	public SyntaxToken mSemicolonToken;

	public LocalDeclarationStatementSyntax(SyntaxList<SyntaxToken> Modifiers, VariableDeclarationSyntax Declaration, SyntaxToken SemicolonToken)
	{
		this.mModifiers = Modifiers;
		this.mDeclaration = Declaration;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mModifiers;
		case 1:
			return mDeclaration;
		case 2:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitLocalDeclarationStatementSyntax(this);
	}
}

// 变量声明
public class VariableDeclarationSyntax : SyntaxNode
{
	// 类型
	public TypeSyntax mType;

	// 变量表
	public SeparatedSyntaxList<VariableDeclaratorSyntax> mVariables;

	public VariableDeclarationSyntax(TypeSyntax Type, SeparatedSyntaxList<VariableDeclaratorSyntax> Variables)
	{
		this.mType = Type;
		this.mVariables = Variables;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mType;
		case 1:
			return mVariables;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitVariableDeclarationSyntax(this);
	}
}

// 变量定义
public class VariableDeclaratorSyntax : SyntaxNode
{
	// 名称
	public SyntaxToken mIdentifier;

	// 参数表
	public BracketedArgumentListSyntax mArgumentList;

	// 初始化表
	public EqualsValueClauseSyntax mInitializer;

	public VariableDeclaratorSyntax(SyntaxToken Identifier, BracketedArgumentListSyntax ArgumentList, EqualsValueClauseSyntax Initializer)
	{
		this.mIdentifier = Identifier;
		this.mArgumentList = ArgumentList;
		this.mInitializer = Initializer;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mIdentifier;
		case 1:
			return mArgumentList;
		case 2:
			return mInitializer;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitVariableDeclaratorSyntax(this);
	}
}

// 赋初值
public class EqualsValueClauseSyntax : SyntaxNode
{
	// 等号
	public SyntaxToken mEqualsToken;

	// 值
	public ExpressionSyntax mValue;

	public EqualsValueClauseSyntax(SyntaxToken EqualsToken, ExpressionSyntax Value)
	{
		this.mEqualsToken = EqualsToken;
		this.mValue = Value;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mEqualsToken;
		case 1:
			return mValue;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitEqualsValueClauseSyntax(this);
	}
}

// 变量指定
public abstract class VariableDesignationSyntax : SyntaxNode
{

	public VariableDesignationSyntax()
	{
	}
}

// 简单变量指定
public class SingleVariableDesignationSyntax : VariableDesignationSyntax
{
	public SyntaxToken mIdentifier;

	public SingleVariableDesignationSyntax(SyntaxToken Identifier)
	{
		this.mIdentifier = Identifier;
	}

	public override int Count => 1;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mIdentifier;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitSingleVariableDesignationSyntax(this);
	}
}

// 表达式语句
public class ExpressionStatementSyntax : StatementSyntax
{
	// 表达式
	public ExpressionSyntax mExpression;

	// 分号
	public SyntaxToken mSemicolonToken;

	public ExpressionStatementSyntax(ExpressionSyntax Expression, SyntaxToken SemicolonToken)
	{
		this.mExpression = Expression;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mExpression;
		case 1:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitExpressionStatementSyntax(this);
	}
}

// 空语句
public class EmptyStatementSyntax : StatementSyntax
{
	// 分号
	public SyntaxToken mSemicolonToken;

	public EmptyStatementSyntax(SyntaxToken SemicolonToken)
	{
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 1;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitEmptyStatementSyntax(this);
	}
}

// 标签语句
public class LabeledStatementSyntax : StatementSyntax
{
	// 标签名
	public SyntaxToken mIdentifier;

	// 冒号
	public SyntaxToken mColonToken;

	// 语句
	public StatementSyntax mStatement;

	public LabeledStatementSyntax(SyntaxToken Identifier, SyntaxToken ColonToken, StatementSyntax Statement)
	{
		this.mIdentifier = Identifier;
		this.mColonToken = ColonToken;
		this.mStatement = Statement;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mIdentifier;
		case 1:
			return mColonToken;
		case 2:
			return mStatement;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitLabeledStatementSyntax(this);
	}
}

// goto语句
public class GotoStatementSyntax : StatementSyntax
{
	// goto
	public SyntaxToken mGotoKeyword;

	// case 或 default关键字
	public SyntaxToken mCaseOrDefaultKeyword;

	// 表达式
	public ExpressionSyntax mExpression;

	// 分号
	public SyntaxToken mSemicolonToken;

	public GotoStatementSyntax(SyntaxToken GotoKeyword, SyntaxToken CaseOrDefaultKeyword, ExpressionSyntax Expression, SyntaxToken SemicolonToken)
	{
		this.mGotoKeyword = GotoKeyword;
		this.mCaseOrDefaultKeyword = CaseOrDefaultKeyword;
		this.mExpression = Expression;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 4;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mGotoKeyword;
		case 1:
			return mCaseOrDefaultKeyword;
		case 2:
			return mExpression;
		case 3:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitGotoStatementSyntax(this);
	}
}

// break语句
public class BreakStatementSyntax : StatementSyntax
{
	// break
	public SyntaxToken mBreakKeyword;

	// 分号
	public SyntaxToken mSemicolonToken;

	public BreakStatementSyntax(SyntaxToken BreakKeyword, SyntaxToken SemicolonToken)
	{
		this.mBreakKeyword = BreakKeyword;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mBreakKeyword;
		case 1:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBreakStatementSyntax(this);
	}
}

// continue语句
public class ContinueStatementSyntax : StatementSyntax
{
	// continue
	public SyntaxToken mContinueKeyword;

	// 分号
	public SyntaxToken mSemicolonToken;

	public ContinueStatementSyntax(SyntaxToken ContinueKeyword, SyntaxToken SemicolonToken)
	{
		this.mContinueKeyword = ContinueKeyword;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mContinueKeyword;
		case 1:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitContinueStatementSyntax(this);
	}
}

// return语句
public class ReturnStatementSyntax : StatementSyntax
{
	// return
	public SyntaxToken mReturnKeyword;

	// 返回值
	public ExpressionSyntax mExpression;

	// 分号
	public SyntaxToken mSemicolonToken;

	public ReturnStatementSyntax(SyntaxToken ReturnKeyword, ExpressionSyntax Expression, SyntaxToken SemicolonToken)
	{
		this.mReturnKeyword = ReturnKeyword;
		this.mExpression = Expression;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mReturnKeyword;
		case 1:
			return mExpression;
		case 2:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitReturnStatementSyntax(this);
	}
}

// throw语句
public class ThrowStatementSyntax : StatementSyntax
{
	// throw
	public SyntaxToken mThrowKeyword;

	// 表达式
	public ExpressionSyntax mExpression;

	// 分号
	public SyntaxToken mSemicolonToken;

	public ThrowStatementSyntax(SyntaxToken ThrowKeyword, ExpressionSyntax Expression, SyntaxToken SemicolonToken)
	{
		this.mThrowKeyword = ThrowKeyword;
		this.mExpression = Expression;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mThrowKeyword;
		case 1:
			return mExpression;
		case 2:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitThrowStatementSyntax(this);
	}
}

// while语句
public class WhileStatementSyntax : StatementSyntax
{
	// while
	public SyntaxToken mWhileKeyword;

	// 开括号
	public SyntaxToken mOpenParenToken;

	// 条件表达式
	public ExpressionSyntax mCondition;

	// 关括号
	public SyntaxToken mCloseParenToken;

	// 循环体
	public StatementSyntax mStatement;

	public WhileStatementSyntax(SyntaxToken WhileKeyword, SyntaxToken OpenParenToken, ExpressionSyntax Condition, SyntaxToken CloseParenToken, StatementSyntax Statement)
	{
		this.mWhileKeyword = WhileKeyword;
		this.mOpenParenToken = OpenParenToken;
		this.mCondition = Condition;
		this.mCloseParenToken = CloseParenToken;
		this.mStatement = Statement;
	}

	public override int Count => 5;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mWhileKeyword;
		case 1:
			return mOpenParenToken;
		case 2:
			return mCondition;
		case 3:
			return mCloseParenToken;
		case 4:
			return mStatement;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitWhileStatementSyntax(this);
	}
}

// do-while语句
public class DoStatementSyntax : StatementSyntax
{
	// do
	public SyntaxToken mDoKeyword;

	// 循环体
	public StatementSyntax mStatement;

	// while
	public SyntaxToken mWhileKeyword;

	// 开括号
	public SyntaxToken mOpenParenToken;

	// 条件表达式
	public ExpressionSyntax mCondition;

	// 关括号
	public SyntaxToken mCloseParenToken;

	// 分号
	public SyntaxToken mSemicolonToken;

	public DoStatementSyntax(SyntaxToken DoKeyword, StatementSyntax Statement, SyntaxToken WhileKeyword, SyntaxToken OpenParenToken, ExpressionSyntax Condition, SyntaxToken CloseParenToken, SyntaxToken SemicolonToken)
	{
		this.mDoKeyword = DoKeyword;
		this.mStatement = Statement;
		this.mWhileKeyword = WhileKeyword;
		this.mOpenParenToken = OpenParenToken;
		this.mCondition = Condition;
		this.mCloseParenToken = CloseParenToken;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 7;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mDoKeyword;
		case 1:
			return mStatement;
		case 2:
			return mWhileKeyword;
		case 3:
			return mOpenParenToken;
		case 4:
			return mCondition;
		case 5:
			return mCloseParenToken;
		case 6:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDoStatementSyntax(this);
	}
}

// for语句
public class ForStatementSyntax : StatementSyntax
{
	// for
	public SyntaxToken mForKeyword;

	// 开括号
	public SyntaxToken mOpenParenToken;

	// 变量声明
	public VariableDeclarationSyntax mDeclaration;

	// 初始化表达式
	public SeparatedSyntaxList<ExpressionSyntax> mInitializers;

	// 第一个分号
	public SyntaxToken mFirstSemicolonToken;

	// 条件表达式
	public ExpressionSyntax mCondition;

	// 第二个分号
	public SyntaxToken mSecondSemicolonToken;

	// 递增表达式
	public SeparatedSyntaxList<ExpressionSyntax> mIncrementors;

	// 关括号
	public SyntaxToken mCloseParenToken;

	// 循环体
	public StatementSyntax mStatement;

	public ForStatementSyntax(SyntaxToken ForKeyword, SyntaxToken OpenParenToken, VariableDeclarationSyntax Declaration, SeparatedSyntaxList<ExpressionSyntax> Initializers, SyntaxToken FirstSemicolonToken, ExpressionSyntax Condition, SyntaxToken SecondSemicolonToken, SeparatedSyntaxList<ExpressionSyntax> Incrementors, SyntaxToken CloseParenToken, StatementSyntax Statement)
	{
		this.mForKeyword = ForKeyword;
		this.mOpenParenToken = OpenParenToken;
		this.mDeclaration = Declaration;
		this.mInitializers = Initializers;
		this.mFirstSemicolonToken = FirstSemicolonToken;
		this.mCondition = Condition;
		this.mSecondSemicolonToken = SecondSemicolonToken;
		this.mIncrementors = Incrementors;
		this.mCloseParenToken = CloseParenToken;
		this.mStatement = Statement;
	}

	public override int Count => 10;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mForKeyword;
		case 1:
			return mOpenParenToken;
		case 2:
			return mDeclaration;
		case 3:
			return mInitializers;
		case 4:
			return mFirstSemicolonToken;
		case 5:
			return mCondition;
		case 6:
			return mSecondSemicolonToken;
		case 7:
			return mIncrementors;
		case 8:
			return mCloseParenToken;
		case 9:
			return mStatement;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitForStatementSyntax(this);
	}
}

// foreach语句
public class ForEachStatementSyntax : StatementSyntax
{
	// foreach
	public SyntaxToken mForEachKeyword;

	// 开括号
	public SyntaxToken mOpenParenToken;

	// 变量类型
	public TypeSyntax mType;

	// 变量名
	public SyntaxToken mIdentifier;

	// in或冒号
	public SyntaxToken mInKeyword;

	// 表达式
	public ExpressionSyntax mExpression;

	// 关括号
	public SyntaxToken mCloseParenToken;

	// 循环体
	public StatementSyntax mStatement;

	public ForEachStatementSyntax(SyntaxToken ForEachKeyword, SyntaxToken OpenParenToken, TypeSyntax Type, SyntaxToken Identifier, SyntaxToken InKeyword, ExpressionSyntax Expression, SyntaxToken CloseParenToken, StatementSyntax Statement)
	{
		this.mForEachKeyword = ForEachKeyword;
		this.mOpenParenToken = OpenParenToken;
		this.mType = Type;
		this.mIdentifier = Identifier;
		this.mInKeyword = InKeyword;
		this.mExpression = Expression;
		this.mCloseParenToken = CloseParenToken;
		this.mStatement = Statement;
	}

	public override int Count => 8;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mForEachKeyword;
		case 1:
			return mOpenParenToken;
		case 2:
			return mType;
		case 3:
			return mIdentifier;
		case 4:
			return mInKeyword;
		case 5:
			return mExpression;
		case 6:
			return mCloseParenToken;
		case 7:
			return mStatement;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitForEachStatementSyntax(this);
	}
}

// if语句
public class IfStatementSyntax : StatementSyntax
{
	// if
	public SyntaxToken mIfKeyword;

	// 开括号
	public SyntaxToken mOpenParenToken;

	// 条件表达式
	public ExpressionSyntax mCondition;

	// 关括号
	public SyntaxToken mCloseParenToken;

	// 语句体
	public StatementSyntax mStatement;

	// else语句
	public ElseClauseSyntax mElse;

	public IfStatementSyntax(SyntaxToken IfKeyword, SyntaxToken OpenParenToken, ExpressionSyntax Condition, SyntaxToken CloseParenToken, StatementSyntax Statement, ElseClauseSyntax Else)
	{
		this.mIfKeyword = IfKeyword;
		this.mOpenParenToken = OpenParenToken;
		this.mCondition = Condition;
		this.mCloseParenToken = CloseParenToken;
		this.mStatement = Statement;
		this.mElse = Else;
	}

	public override int Count => 6;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mIfKeyword;
		case 1:
			return mOpenParenToken;
		case 2:
			return mCondition;
		case 3:
			return mCloseParenToken;
		case 4:
			return mStatement;
		case 5:
			return mElse;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitIfStatementSyntax(this);
	}
}

// else语句
public class ElseClauseSyntax : SyntaxNode
{
	// else
	public SyntaxToken mElseKeyword;

	// 语句体
	public StatementSyntax mStatement;

	public ElseClauseSyntax(SyntaxToken ElseKeyword, StatementSyntax Statement)
	{
		this.mElseKeyword = ElseKeyword;
		this.mStatement = Statement;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mElseKeyword;
		case 1:
			return mStatement;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitElseClauseSyntax(this);
	}
}

// switch语句
public class SwitchStatementSyntax : StatementSyntax
{
	// switch
	public SyntaxToken mSwitchKeyword;

	// 开括号
	public SyntaxToken mOpenParenToken;

	// 表达式
	public ExpressionSyntax mExpression;

	// 关括号
	public SyntaxToken mCloseParenToken;

	// 开大括号
	public SyntaxToken mOpenBraceToken;

	// case区段
	public SyntaxList<SwitchSectionSyntax> mSections;

	// 关大括号
	public SyntaxToken mCloseBraceToken;

	public SwitchStatementSyntax(SyntaxToken SwitchKeyword, SyntaxToken OpenParenToken, ExpressionSyntax Expression, SyntaxToken CloseParenToken, SyntaxToken OpenBraceToken, SyntaxList<SwitchSectionSyntax> Sections, SyntaxToken CloseBraceToken)
	{
		this.mSwitchKeyword = SwitchKeyword;
		this.mOpenParenToken = OpenParenToken;
		this.mExpression = Expression;
		this.mCloseParenToken = CloseParenToken;
		this.mOpenBraceToken = OpenBraceToken;
		this.mSections = Sections;
		this.mCloseBraceToken = CloseBraceToken;
	}

	public override int Count => 7;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mSwitchKeyword;
		case 1:
			return mOpenParenToken;
		case 2:
			return mExpression;
		case 3:
			return mCloseParenToken;
		case 4:
			return mOpenBraceToken;
		case 5:
			return mSections;
		case 6:
			return mCloseBraceToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitSwitchStatementSyntax(this);
	}
}

// switch-case区段
public class SwitchSectionSyntax : SyntaxNode
{
	// case标签
	public SyntaxList<SwitchLabelSyntax> mLabels;

	// 语句列表
	public SyntaxList<StatementSyntax> mStatements;

	public SwitchSectionSyntax(SyntaxList<SwitchLabelSyntax> Labels, SyntaxList<StatementSyntax> Statements)
	{
		this.mLabels = Labels;
		this.mStatements = Statements;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mLabels;
		case 1:
			return mStatements;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitSwitchSectionSyntax(this);
	}
}

// case标签
public class SwitchLabelSyntax : SyntaxNode
{
	// case或default
	public SyntaxToken mKeyword;

	// 值
	public ExpressionSyntax mValue;

	// 分号
	public SyntaxToken mColonToken;

	public SwitchLabelSyntax(SyntaxToken Keyword, ExpressionSyntax Value, SyntaxToken ColonToken)
	{
		this.mKeyword = Keyword;
		this.mValue = Value;
		this.mColonToken = ColonToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mKeyword;
		case 1:
			return mValue;
		case 2:
			return mColonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitSwitchLabelSyntax(this);
	}
}

// try语句
public class TryStatementSyntax : StatementSyntax
{
	// try
	public SyntaxToken mTryKeyword;

	// 语句体
	public BlockSyntax mBlock;

	// Catch表
	public SyntaxList<CatchClauseSyntax> mCatches;

	// Finally体
	public FinallyClauseSyntax mFinally;

	public TryStatementSyntax(SyntaxToken TryKeyword, BlockSyntax Block, SyntaxList<CatchClauseSyntax> Catches, FinallyClauseSyntax Finally)
	{
		this.mTryKeyword = TryKeyword;
		this.mBlock = Block;
		this.mCatches = Catches;
		this.mFinally = Finally;
	}

	public override int Count => 4;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mTryKeyword;
		case 1:
			return mBlock;
		case 2:
			return mCatches;
		case 3:
			return mFinally;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTryStatementSyntax(this);
	}
}

// Catch语句
public class CatchClauseSyntax : SyntaxNode
{
	// catch
	public SyntaxToken mCatchKeyword;

	// 异常描述
	public CatchDeclarationSyntax mDeclaration;

	// Catch体
	public BlockSyntax mBlock;

	public CatchClauseSyntax(SyntaxToken CatchKeyword, CatchDeclarationSyntax Declaration, BlockSyntax Block)
	{
		this.mCatchKeyword = CatchKeyword;
		this.mDeclaration = Declaration;
		this.mBlock = Block;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mCatchKeyword;
		case 1:
			return mDeclaration;
		case 2:
			return mBlock;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitCatchClauseSyntax(this);
	}
}

// 异常描述
public class CatchDeclarationSyntax : SyntaxNode
{
	// 开括号
	public SyntaxToken mOpenParenToken;

	// 类型
	public TypeSyntax mType;

	// 名称
	public SyntaxToken mIdentifier;

	// 关括号
	public SyntaxToken mCloseParenToken;

	public CatchDeclarationSyntax(SyntaxToken OpenParenToken, TypeSyntax Type, SyntaxToken Identifier, SyntaxToken CloseParenToken)
	{
		this.mOpenParenToken = OpenParenToken;
		this.mType = Type;
		this.mIdentifier = Identifier;
		this.mCloseParenToken = CloseParenToken;
	}

	public override int Count => 4;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mOpenParenToken;
		case 1:
			return mType;
		case 2:
			return mIdentifier;
		case 3:
			return mCloseParenToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitCatchDeclarationSyntax(this);
	}
}

// Finally体
public class FinallyClauseSyntax : SyntaxNode
{
	// finally
	public SyntaxToken mFinallyKeyword;

	// Finally体
	public BlockSyntax mBlock;

	public FinallyClauseSyntax(SyntaxToken FinallyKeyword, BlockSyntax Block)
	{
		this.mFinallyKeyword = FinallyKeyword;
		this.mBlock = Block;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mFinallyKeyword;
		case 1:
			return mBlock;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFinallyClauseSyntax(this);
	}
}

// 文件单元
public class FileUnitSyntax : SyntaxNode
{
	// using语句
	public SyntaxList<UsingDirectiveSyntax> mUsings;

	// 属性表
	public SyntaxList<AttributeListSyntax> mAttributeLists;

	// 成员
	public SyntaxList<MemberDeclarationSyntax> mMembers;

	public FileUnitSyntax(SyntaxList<UsingDirectiveSyntax> Usings, SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<MemberDeclarationSyntax> Members)
	{
		this.mUsings = Usings;
		this.mAttributeLists = AttributeLists;
		this.mMembers = Members;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mUsings;
		case 1:
			return mAttributeLists;
		case 2:
			return mMembers;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFileUnitSyntax(this);
	}
}

// using声明
public class UsingDirectiveSyntax : SyntaxNode
{
	// using
	public SyntaxToken mUsingKeyword;

	// 别名
	public NameEqualsSyntax mAlias;

	// 引用目标名
	public NameSyntax mName;

	// 分号
	public SyntaxToken mSemicolonToken;

	public UsingDirectiveSyntax(SyntaxToken UsingKeyword, NameEqualsSyntax Alias, NameSyntax Name, SyntaxToken SemicolonToken)
	{
		this.mUsingKeyword = UsingKeyword;
		this.mAlias = Alias;
		this.mName = Name;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 4;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mUsingKeyword;
		case 1:
			return mAlias;
		case 2:
			return mName;
		case 3:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitUsingDirectiveSyntax(this);
	}
}

// 成员定义
public abstract class MemberDeclarationSyntax : SyntaxNode
{

	public MemberDeclarationSyntax()
	{
	}
}

// 命名空间定义
public class NamespaceDeclarationSyntax : MemberDeclarationSyntax
{
	// namespace
	public SyntaxToken mNamespaceKeyword;

	// 名称
	public NameSyntax mName;

	// 开大括号
	public SyntaxToken mOpenBraceToken;

	// 引用表
	public SyntaxList<UsingDirectiveSyntax> mUsings;

	// 成员
	public SyntaxList<MemberDeclarationSyntax> mMembers;

	// 关大括号
	public SyntaxToken mCloseBraceToken;

	// 分号
	public SyntaxToken mSemicolonToken;

	public NamespaceDeclarationSyntax(SyntaxToken NamespaceKeyword, NameSyntax Name, SyntaxToken OpenBraceToken, SyntaxList<UsingDirectiveSyntax> Usings, SyntaxList<MemberDeclarationSyntax> Members, SyntaxToken CloseBraceToken, SyntaxToken SemicolonToken)
	{
		this.mNamespaceKeyword = NamespaceKeyword;
		this.mName = Name;
		this.mOpenBraceToken = OpenBraceToken;
		this.mUsings = Usings;
		this.mMembers = Members;
		this.mCloseBraceToken = CloseBraceToken;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 7;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mNamespaceKeyword;
		case 1:
			return mName;
		case 2:
			return mOpenBraceToken;
		case 3:
			return mUsings;
		case 4:
			return mMembers;
		case 5:
			return mCloseBraceToken;
		case 6:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitNamespaceDeclarationSyntax(this);
	}
}

// 属性定义
public class AttributeListSyntax : SyntaxNode
{
	// @符号
	public SyntaxToken mAtToken;

	// 名称
	public NameSyntax mName;

	// 值
	public EqualsValueClauseSyntax mValue;

	public AttributeListSyntax(SyntaxToken AtToken, NameSyntax Name, EqualsValueClauseSyntax Value)
	{
		this.mAtToken = AtToken;
		this.mName = Name;
		this.mValue = Value;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAtToken;
		case 1:
			return mName;
		case 2:
			return mValue;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitAttributeListSyntax(this);
	}
}

// 名称=
public class NameEqualsSyntax : SyntaxNode
{
	// 名称
	public IdentifierNameSyntax mName;

	// 等号
	public SyntaxToken mEqualsToken;

	public NameEqualsSyntax(IdentifierNameSyntax Name, SyntaxToken EqualsToken)
	{
		this.mName = Name;
		this.mEqualsToken = EqualsToken;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mName;
		case 1:
			return mEqualsToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitNameEqualsSyntax(this);
	}
}

// 类型形参列表
public class TypeParameterListSyntax : SyntaxNode
{
	// 左尖括号
	public SyntaxToken mLessThanToken;

	// 形参表
	public SeparatedSyntaxList<TypeParameterSyntax> mParameters;

	// 右尖括号
	public SyntaxToken mGreaterThanToken;

	public TypeParameterListSyntax(SyntaxToken LessThanToken, SeparatedSyntaxList<TypeParameterSyntax> Parameters, SyntaxToken GreaterThanToken)
	{
		this.mLessThanToken = LessThanToken;
		this.mParameters = Parameters;
		this.mGreaterThanToken = GreaterThanToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mLessThanToken;
		case 1:
			return mParameters;
		case 2:
			return mGreaterThanToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTypeParameterListSyntax(this);
	}
}

// 类型形参
public class TypeParameterSyntax : SyntaxNode
{
	// 形参名
	public SyntaxToken mIdentifier;

	public TypeParameterSyntax(SyntaxToken Identifier)
	{
		this.mIdentifier = Identifier;
	}

	public override int Count => 1;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mIdentifier;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTypeParameterSyntax(this);
	}
}

// 类型定义基类
public abstract class BaseTypeDeclarationSyntax : MemberDeclarationSyntax
{
	// 属性表
	public SyntaxList<AttributeListSyntax> mAttributeLists;

	// 修饰列表
	public SyntaxList<SyntaxToken> mModifiers;

	// 名称
	public SyntaxToken mIdentifier;

	// 基类列表
	public BaseListSyntax mBaseList;

	// 开大括号
	public SyntaxToken mOpenBraceToken;

	// 关大括号
	public SyntaxToken mCloseBraceToken;

	// 分号
	public SyntaxToken mSemicolonToken;

	public BaseTypeDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, SyntaxToken Identifier, BaseListSyntax BaseList, SyntaxToken OpenBraceToken, SyntaxToken CloseBraceToken, SyntaxToken SemicolonToken)
	{
		this.mAttributeLists = AttributeLists;
		this.mModifiers = Modifiers;
		this.mIdentifier = Identifier;
		this.mBaseList = BaseList;
		this.mOpenBraceToken = OpenBraceToken;
		this.mCloseBraceToken = CloseBraceToken;
		this.mSemicolonToken = SemicolonToken;
	}
}

// 类型定义
public abstract class TypeDeclarationSyntax : BaseTypeDeclarationSyntax
{
	// 关键字 ("class", "struct", "interface", "enum").
	public SyntaxToken mKeyword;

	// 类型形参
	public TypeParameterListSyntax mTypeParameterList;

	// 类成员
	public SyntaxList<MemberDeclarationSyntax> mMembers;

	public TypeDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, SyntaxToken Identifier, BaseListSyntax BaseList, SyntaxToken OpenBraceToken, SyntaxToken CloseBraceToken, SyntaxToken SemicolonToken, SyntaxToken Keyword, TypeParameterListSyntax TypeParameterList, SyntaxList<MemberDeclarationSyntax> Members)
		: base(AttributeLists, Modifiers, Identifier, BaseList, OpenBraceToken, CloseBraceToken, SemicolonToken)
	{
		this.mKeyword = Keyword;
		this.mTypeParameterList = TypeParameterList;
		this.mMembers = Members;
	}
}

// 类定义
public class ClassDeclarationSyntax : TypeDeclarationSyntax
{

	public ClassDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, SyntaxToken Identifier, BaseListSyntax BaseList, SyntaxToken OpenBraceToken, SyntaxToken CloseBraceToken, SyntaxToken SemicolonToken, SyntaxToken Keyword, TypeParameterListSyntax TypeParameterList, SyntaxList<MemberDeclarationSyntax> Members)
		: base(AttributeLists, Modifiers, Identifier, BaseList, OpenBraceToken, CloseBraceToken, SemicolonToken, Keyword, TypeParameterList, Members)
	{
	}

	public override int Count => 10;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mIdentifier;
		case 3:
			return mBaseList;
		case 4:
			return mOpenBraceToken;
		case 5:
			return mCloseBraceToken;
		case 6:
			return mSemicolonToken;
		case 7:
			return mKeyword;
		case 8:
			return mTypeParameterList;
		case 9:
			return mMembers;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitClassDeclarationSyntax(this);
	}
}

// 结构体定义
public class StructDeclarationSyntax : TypeDeclarationSyntax
{

	public StructDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, SyntaxToken Identifier, BaseListSyntax BaseList, SyntaxToken OpenBraceToken, SyntaxToken CloseBraceToken, SyntaxToken SemicolonToken, SyntaxToken Keyword, TypeParameterListSyntax TypeParameterList, SyntaxList<MemberDeclarationSyntax> Members)
		: base(AttributeLists, Modifiers, Identifier, BaseList, OpenBraceToken, CloseBraceToken, SemicolonToken, Keyword, TypeParameterList, Members)
	{
	}

	public override int Count => 10;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mIdentifier;
		case 3:
			return mBaseList;
		case 4:
			return mOpenBraceToken;
		case 5:
			return mCloseBraceToken;
		case 6:
			return mSemicolonToken;
		case 7:
			return mKeyword;
		case 8:
			return mTypeParameterList;
		case 9:
			return mMembers;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitStructDeclarationSyntax(this);
	}
}

// 接口定义
public class InterfaceDeclarationSyntax : TypeDeclarationSyntax
{

	public InterfaceDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, SyntaxToken Identifier, BaseListSyntax BaseList, SyntaxToken OpenBraceToken, SyntaxToken CloseBraceToken, SyntaxToken SemicolonToken, SyntaxToken Keyword, TypeParameterListSyntax TypeParameterList, SyntaxList<MemberDeclarationSyntax> Members)
		: base(AttributeLists, Modifiers, Identifier, BaseList, OpenBraceToken, CloseBraceToken, SemicolonToken, Keyword, TypeParameterList, Members)
	{
	}

	public override int Count => 10;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mIdentifier;
		case 3:
			return mBaseList;
		case 4:
			return mOpenBraceToken;
		case 5:
			return mCloseBraceToken;
		case 6:
			return mSemicolonToken;
		case 7:
			return mKeyword;
		case 8:
			return mTypeParameterList;
		case 9:
			return mMembers;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitInterfaceDeclarationSyntax(this);
	}
}

// 枚举类型
public class EnumDeclarationSyntax : TypeDeclarationSyntax
{
	// 枚举成员列表
	public SeparatedSyntaxList<EnumMemberDeclarationSyntax> mMembers;

	public EnumDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, SyntaxToken Identifier, BaseListSyntax BaseList, SyntaxToken OpenBraceToken, SyntaxToken CloseBraceToken, SyntaxToken SemicolonToken, SyntaxToken Keyword, TypeParameterListSyntax TypeParameterList, SyntaxList<MemberDeclarationSyntax> Members, SeparatedSyntaxList<EnumMemberDeclarationSyntax> Members)
		: base(AttributeLists, Modifiers, Identifier, BaseList, OpenBraceToken, CloseBraceToken, SemicolonToken, Keyword, TypeParameterList, Members)
	{
		this.mMembers = Members;
	}

	public override int Count => 11;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mIdentifier;
		case 3:
			return mBaseList;
		case 4:
			return mOpenBraceToken;
		case 5:
			return mCloseBraceToken;
		case 6:
			return mSemicolonToken;
		case 7:
			return mKeyword;
		case 8:
			return mTypeParameterList;
		case 9:
			return mMembers;
		case 10:
			return mMembers;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitEnumDeclarationSyntax(this);
	}
}

// 委托类型
public class DelegateDeclarationSyntax : MemberDeclarationSyntax
{
	// 属性表
	public SyntaxList<AttributeListSyntax> mAttributeLists;

	// 修饰列表
	public SyntaxList<SyntaxToken> mModifiers;

	// delegate
	public SyntaxToken mDelegateKeyword;

	// 返回类型
	public TypeSyntax mReturnType;

	// 名称
	public SyntaxToken mIdentifier;

	// 类型形参
	public TypeParameterListSyntax mTypeParameterList;

	// 方法形参表
	public ParameterListSyntax mParameterList;

	// 分号
	public SyntaxToken mSemicolonToken;

	public DelegateDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, SyntaxToken DelegateKeyword, TypeSyntax ReturnType, SyntaxToken Identifier, TypeParameterListSyntax TypeParameterList, ParameterListSyntax ParameterList, SyntaxToken SemicolonToken)
	{
		this.mAttributeLists = AttributeLists;
		this.mModifiers = Modifiers;
		this.mDelegateKeyword = DelegateKeyword;
		this.mReturnType = ReturnType;
		this.mIdentifier = Identifier;
		this.mTypeParameterList = TypeParameterList;
		this.mParameterList = ParameterList;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 8;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mDelegateKeyword;
		case 3:
			return mReturnType;
		case 4:
			return mIdentifier;
		case 5:
			return mTypeParameterList;
		case 6:
			return mParameterList;
		case 7:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDelegateDeclarationSyntax(this);
	}
}

// 枚举成员
public class EnumMemberDeclarationSyntax : MemberDeclarationSyntax
{
	// 属性表
	public SyntaxList<AttributeListSyntax> mAttributeLists;

	// 名称
	public SyntaxToken mIdentifier;

	// 值
	public EqualsValueClauseSyntax mEqualsValue;

	public EnumMemberDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxToken Identifier, EqualsValueClauseSyntax EqualsValue)
	{
		this.mAttributeLists = AttributeLists;
		this.mIdentifier = Identifier;
		this.mEqualsValue = EqualsValue;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mIdentifier;
		case 2:
			return mEqualsValue;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitEnumMemberDeclarationSyntax(this);
	}
}

// 基类列表
public class BaseListSyntax : SyntaxNode
{
	// 冒号
	public SyntaxToken mColonToken;

	// 类型列表
	public SeparatedSyntaxList<TypeSyntax> mTypes;

	public BaseListSyntax(SyntaxToken ColonToken, SeparatedSyntaxList<TypeSyntax> Types)
	{
		this.mColonToken = ColonToken;
		this.mTypes = Types;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mColonToken;
		case 1:
			return mTypes;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBaseListSyntax(this);
	}
}

// 字段定义
public abstract class FieldDeclarationSyntax : MemberDeclarationSyntax
{
	// 属性表
	public SyntaxList<AttributeListSyntax> mAttributeLists;

	// 修饰列表
	public SyntaxList<SyntaxToken> mModifiers;

	// 字段描述
	public VariableDeclarationSyntax mDeclaration;

	// 分号
	public SyntaxToken mSemicolonToken;

	public FieldDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, VariableDeclarationSyntax Declaration, SyntaxToken SemicolonToken)
	{
		this.mAttributeLists = AttributeLists;
		this.mModifiers = Modifiers;
		this.mDeclaration = Declaration;
		this.mSemicolonToken = SemicolonToken;
	}
}

// 显示接口指定
public class ExplicitInterfaceSpecifierSyntax : SyntaxNode
{
	// 接口名
	public NameSyntax mName;

	// 点号
	public SyntaxToken mDotToken;

	public ExplicitInterfaceSpecifierSyntax(NameSyntax Name, SyntaxToken DotToken)
	{
		this.mName = Name;
		this.mDotToken = DotToken;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mName;
		case 1:
			return mDotToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitExplicitInterfaceSpecifierSyntax(this);
	}
}

// 方法定义基类
public abstract class BaseMethodDeclarationSyntax : MemberDeclarationSyntax
{
	// 属性表
	public SyntaxList<AttributeListSyntax> mAttributeLists;

	// 修饰列表
	public SyntaxList<SyntaxToken> mModifiers;

	// 参数列表
	public ParameterListSyntax mParameterList;

	// 方法体
	public BlockSyntax mBody;

	// 箭头表达式
	public ArrowExpressionClauseSyntax mExpressionBody;

	// 分号
	public SyntaxToken mSemicolonToken;

	public BaseMethodDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, ParameterListSyntax ParameterList, BlockSyntax Body, ArrowExpressionClauseSyntax ExpressionBody, SyntaxToken SemicolonToken)
	{
		this.mAttributeLists = AttributeLists;
		this.mModifiers = Modifiers;
		this.mParameterList = ParameterList;
		this.mBody = Body;
		this.mExpressionBody = ExpressionBody;
		this.mSemicolonToken = SemicolonToken;
	}
}

// 方法定义
public class MethodDeclarationSyntax : BaseMethodDeclarationSyntax
{
	// 返回类型
	public TypeSyntax mReturnType;

	// 显示接口名
	public ExplicitInterfaceSpecifierSyntax mExplicitInterfaceSpecifier;

	// 方法名
	public SyntaxToken mIdentifier;

	// 类型形参
	public TypeParameterListSyntax mTypeParameterList;

	public MethodDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, ParameterListSyntax ParameterList, BlockSyntax Body, ArrowExpressionClauseSyntax ExpressionBody, SyntaxToken SemicolonToken, TypeSyntax ReturnType, ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier, SyntaxToken Identifier, TypeParameterListSyntax TypeParameterList)
		: base(AttributeLists, Modifiers, ParameterList, Body, ExpressionBody, SemicolonToken)
	{
		this.mReturnType = ReturnType;
		this.mExplicitInterfaceSpecifier = ExplicitInterfaceSpecifier;
		this.mIdentifier = Identifier;
		this.mTypeParameterList = TypeParameterList;
	}

	public override int Count => 10;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mParameterList;
		case 3:
			return mBody;
		case 4:
			return mExpressionBody;
		case 5:
			return mSemicolonToken;
		case 6:
			return mReturnType;
		case 7:
			return mExplicitInterfaceSpecifier;
		case 8:
			return mIdentifier;
		case 9:
			return mTypeParameterList;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitMethodDeclarationSyntax(this);
	}
}

// 操作符方法
public class OperatorDeclarationSyntax : BaseMethodDeclarationSyntax
{
	// 返回类型
	public TypeSyntax mReturnType;

	// operator
	public SyntaxToken mOperatorKeyword;

	// 操作符
	public SyntaxToken mOperatorToken;

	public OperatorDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, ParameterListSyntax ParameterList, BlockSyntax Body, ArrowExpressionClauseSyntax ExpressionBody, SyntaxToken SemicolonToken, TypeSyntax ReturnType, SyntaxToken OperatorKeyword, SyntaxToken OperatorToken)
		: base(AttributeLists, Modifiers, ParameterList, Body, ExpressionBody, SemicolonToken)
	{
		this.mReturnType = ReturnType;
		this.mOperatorKeyword = OperatorKeyword;
		this.mOperatorToken = OperatorToken;
	}

	public override int Count => 9;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mParameterList;
		case 3:
			return mBody;
		case 4:
			return mExpressionBody;
		case 5:
			return mSemicolonToken;
		case 6:
			return mReturnType;
		case 7:
			return mOperatorKeyword;
		case 8:
			return mOperatorToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitOperatorDeclarationSyntax(this);
	}
}

// 类型转换操作符
public class ConversionOperatorDeclarationSyntax : BaseMethodDeclarationSyntax
{
	// operator
	public SyntaxToken mOperatorKeyword;

	// 类型
	public TypeSyntax mType;

	public ConversionOperatorDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, ParameterListSyntax ParameterList, BlockSyntax Body, ArrowExpressionClauseSyntax ExpressionBody, SyntaxToken SemicolonToken, SyntaxToken OperatorKeyword, TypeSyntax Type)
		: base(AttributeLists, Modifiers, ParameterList, Body, ExpressionBody, SemicolonToken)
	{
		this.mOperatorKeyword = OperatorKeyword;
		this.mType = Type;
	}

	public override int Count => 8;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mParameterList;
		case 3:
			return mBody;
		case 4:
			return mExpressionBody;
		case 5:
			return mSemicolonToken;
		case 6:
			return mOperatorKeyword;
		case 7:
			return mType;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitConversionOperatorDeclarationSyntax(this);
	}
}

// 构造器
public class ConstructorDeclarationSyntax : BaseMethodDeclarationSyntax
{
	// 名称
	public SyntaxToken mIdentifier;

	// 初始化器
	public ConstructorInitializerSyntax mInitializer;

	public ConstructorDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, ParameterListSyntax ParameterList, BlockSyntax Body, ArrowExpressionClauseSyntax ExpressionBody, SyntaxToken SemicolonToken, SyntaxToken Identifier, ConstructorInitializerSyntax Initializer)
		: base(AttributeLists, Modifiers, ParameterList, Body, ExpressionBody, SemicolonToken)
	{
		this.mIdentifier = Identifier;
		this.mInitializer = Initializer;
	}

	public override int Count => 8;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mParameterList;
		case 3:
			return mBody;
		case 4:
			return mExpressionBody;
		case 5:
			return mSemicolonToken;
		case 6:
			return mIdentifier;
		case 7:
			return mInitializer;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitConstructorDeclarationSyntax(this);
	}
}

// 构造器初始化器
public class ConstructorInitializerSyntax : SyntaxNode
{
	// 冒号
	public SyntaxToken mColonToken;

	// this 或 base
	public SyntaxToken mThisOrBaseKeyword;

	// 参数表
	public ArgumentListSyntax mArgumentList;

	public ConstructorInitializerSyntax(SyntaxToken ColonToken, SyntaxToken ThisOrBaseKeyword, ArgumentListSyntax ArgumentList)
	{
		this.mColonToken = ColonToken;
		this.mThisOrBaseKeyword = ThisOrBaseKeyword;
		this.mArgumentList = ArgumentList;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mColonToken;
		case 1:
			return mThisOrBaseKeyword;
		case 2:
			return mArgumentList;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitConstructorInitializerSyntax(this);
	}
}

// 析构器
public class DestructorDeclarationSyntax : BaseMethodDeclarationSyntax
{
	// 破浪号
	public SyntaxToken mTildeToken;

	// 名称
	public SyntaxToken mIdentifier;

	public DestructorDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, ParameterListSyntax ParameterList, BlockSyntax Body, ArrowExpressionClauseSyntax ExpressionBody, SyntaxToken SemicolonToken, SyntaxToken TildeToken, SyntaxToken Identifier)
		: base(AttributeLists, Modifiers, ParameterList, Body, ExpressionBody, SemicolonToken)
	{
		this.mTildeToken = TildeToken;
		this.mIdentifier = Identifier;
	}

	public override int Count => 8;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mParameterList;
		case 3:
			return mBody;
		case 4:
			return mExpressionBody;
		case 5:
			return mSemicolonToken;
		case 6:
			return mTildeToken;
		case 7:
			return mIdentifier;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDestructorDeclarationSyntax(this);
	}
}

// 属性定义基类
public abstract class BasePropertyDeclarationSyntax : MemberDeclarationSyntax
{
	// 属性表
	public SyntaxList<AttributeListSyntax> mAttributeLists;

	// 修饰列表
	public SyntaxList<SyntaxToken> mModifiers;

	// 类型
	public TypeSyntax mType;

	// 显示接口名
	public ExplicitInterfaceSpecifierSyntax mExplicitInterfaceSpecifier;

	// 属性体
	public AccessorListSyntax mAccessorList;

	public BasePropertyDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, TypeSyntax Type, ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier, AccessorListSyntax AccessorList)
	{
		this.mAttributeLists = AttributeLists;
		this.mModifiers = Modifiers;
		this.mType = Type;
		this.mExplicitInterfaceSpecifier = ExplicitInterfaceSpecifier;
		this.mAccessorList = AccessorList;
	}
}

// 属性定义
public class PropertyDeclarationSyntax : BasePropertyDeclarationSyntax
{
	// 名称
	public SyntaxToken mIdentifier;

	// 箭头表达式
	public ArrowExpressionClauseSyntax mExpressionBody;

	// 初始化
	public EqualsValueClauseSyntax mInitializer;

	// 分号
	public SyntaxToken mSemicolonToken;

	public PropertyDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, TypeSyntax Type, ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier, AccessorListSyntax AccessorList, SyntaxToken Identifier, ArrowExpressionClauseSyntax ExpressionBody, EqualsValueClauseSyntax Initializer, SyntaxToken SemicolonToken)
		: base(AttributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, AccessorList)
	{
		this.mIdentifier = Identifier;
		this.mExpressionBody = ExpressionBody;
		this.mInitializer = Initializer;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 9;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mType;
		case 3:
			return mExplicitInterfaceSpecifier;
		case 4:
			return mAccessorList;
		case 5:
			return mIdentifier;
		case 6:
			return mExpressionBody;
		case 7:
			return mInitializer;
		case 8:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPropertyDeclarationSyntax(this);
	}
}

// 箭头表达式
public class ArrowExpressionClauseSyntax : SyntaxNode
{
	// 箭头
	public SyntaxToken mArrowToken;

	// 表达式
	public ExpressionSyntax mExpression;

	public ArrowExpressionClauseSyntax(SyntaxToken ArrowToken, ExpressionSyntax Expression)
	{
		this.mArrowToken = ArrowToken;
		this.mExpression = Expression;
	}

	public override int Count => 2;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mArrowToken;
		case 1:
			return mExpression;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitArrowExpressionClauseSyntax(this);
	}
}

// 下标访问
public class IndexerDeclarationSyntax : BasePropertyDeclarationSyntax
{
	// this
	public SyntaxToken mThisKeyword;

	// 方括号形参列表
	public BracketedParameterListSyntax mParameterList;

	// 箭头表达式
	public ArrowExpressionClauseSyntax mExpressionBody;

	// 分号
	public SyntaxToken mSemicolonToken;

	public IndexerDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, TypeSyntax Type, ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier, AccessorListSyntax AccessorList, SyntaxToken ThisKeyword, BracketedParameterListSyntax ParameterList, ArrowExpressionClauseSyntax ExpressionBody, SyntaxToken SemicolonToken)
		: base(AttributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, AccessorList)
	{
		this.mThisKeyword = ThisKeyword;
		this.mParameterList = ParameterList;
		this.mExpressionBody = ExpressionBody;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 9;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mType;
		case 3:
			return mExplicitInterfaceSpecifier;
		case 4:
			return mAccessorList;
		case 5:
			return mThisKeyword;
		case 6:
			return mParameterList;
		case 7:
			return mExpressionBody;
		case 8:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitIndexerDeclarationSyntax(this);
	}
}

// 属性体
public class AccessorListSyntax : SyntaxNode
{
	// 开大括号
	public SyntaxToken mOpenBraceToken;

	// 属性方法表
	public SyntaxList<AccessorDeclarationSyntax> mAccessors;

	// 关大括号
	public SyntaxToken mCloseBraceToken;

	public AccessorListSyntax(SyntaxToken OpenBraceToken, SyntaxList<AccessorDeclarationSyntax> Accessors, SyntaxToken CloseBraceToken)
	{
		this.mOpenBraceToken = OpenBraceToken;
		this.mAccessors = Accessors;
		this.mCloseBraceToken = CloseBraceToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mOpenBraceToken;
		case 1:
			return mAccessors;
		case 2:
			return mCloseBraceToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitAccessorListSyntax(this);
	}
}

// 属性方法
public class AccessorDeclarationSyntax : SyntaxNode
{
	// 属性表
	public SyntaxList<AttributeListSyntax> mAttributeLists;

	// 修饰列表
	public SyntaxList<SyntaxToken> mModifiers;

	// 方法关键字(get,set,add,remove)
	public SyntaxToken mKeyword;

	// 方法体
	public BlockSyntax mBody;

	// 箭头表达式
	public ArrowExpressionClauseSyntax mExpressionBody;

	// 分号
	public SyntaxToken mSemicolonToken;

	public AccessorDeclarationSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, SyntaxToken Keyword, BlockSyntax Body, ArrowExpressionClauseSyntax ExpressionBody, SyntaxToken SemicolonToken)
	{
		this.mAttributeLists = AttributeLists;
		this.mModifiers = Modifiers;
		this.mKeyword = Keyword;
		this.mBody = Body;
		this.mExpressionBody = ExpressionBody;
		this.mSemicolonToken = SemicolonToken;
	}

	public override int Count => 6;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mKeyword;
		case 3:
			return mBody;
		case 4:
			return mExpressionBody;
		case 5:
			return mSemicolonToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitAccessorDeclarationSyntax(this);
	}
}

// 形参表基类
public abstract class BaseParameterListSyntax : SyntaxNode
{
	// 形参
	public SeparatedSyntaxList<ParameterSyntax> mParameters;

	public BaseParameterListSyntax(SeparatedSyntaxList<ParameterSyntax> Parameters)
	{
		this.mParameters = Parameters;
	}
}

// 形参表
public class ParameterListSyntax : BaseParameterListSyntax
{
	// 开括号
	public SyntaxToken mOpenParenToken;

	// 关括号
	public SyntaxToken mCloseParenToken;

	public ParameterListSyntax(SeparatedSyntaxList<ParameterSyntax> Parameters, SyntaxToken OpenParenToken, SyntaxToken CloseParenToken)
		: base(Parameters)
	{
		this.mOpenParenToken = OpenParenToken;
		this.mCloseParenToken = CloseParenToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mParameters;
		case 1:
			return mOpenParenToken;
		case 2:
			return mCloseParenToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitParameterListSyntax(this);
	}
}

// 方括号形参列表
public class BracketedParameterListSyntax : BaseParameterListSyntax
{
	// 开方括号
	public SyntaxToken mOpenBracketToken;

	// 关方括号
	public SyntaxToken mCloseBracketToken;

	public BracketedParameterListSyntax(SeparatedSyntaxList<ParameterSyntax> Parameters, SyntaxToken OpenBracketToken, SyntaxToken CloseBracketToken)
		: base(Parameters)
	{
		this.mOpenBracketToken = OpenBracketToken;
		this.mCloseBracketToken = CloseBracketToken;
	}

	public override int Count => 3;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mParameters;
		case 1:
			return mOpenBracketToken;
		case 2:
			return mCloseBracketToken;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBracketedParameterListSyntax(this);
	}
}

// 形参
public class ParameterSyntax : SyntaxNode
{
	// 属性表
	public SyntaxList<AttributeListSyntax> mAttributeLists;

	// 修饰列表
	public SyntaxList<SyntaxToken> mModifiers;

	// 类型
	public TypeSyntax mType;

	// 名称
	public SyntaxToken mIdentifier;

	// 默认值
	public EqualsValueClauseSyntax mDefault;

	public ParameterSyntax(SyntaxList<AttributeListSyntax> AttributeLists, SyntaxList<SyntaxToken> Modifiers, TypeSyntax Type, SyntaxToken Identifier, EqualsValueClauseSyntax Default)
	{
		this.mAttributeLists = AttributeLists;
		this.mModifiers = Modifiers;
		this.mType = Type;
		this.mIdentifier = Identifier;
		this.mDefault = Default;
	}

	public override int Count => 5;
	public override SyntaxNode GetAt(int index)
	{
		switch (index)
		{
		case 0:
			return mAttributeLists;
		case 1:
			return mModifiers;
		case 2:
			return mType;
		case 3:
			return mIdentifier;
		case 4:
			return mDefault;
		}
		return null;
	}
	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitParameterSyntax(this);
	}
}
