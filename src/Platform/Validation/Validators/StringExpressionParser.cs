using System;
using System.Linq.Expressions;
using System.Text;

namespace Platform.Validation.Validators
{
	public class StringExpressionParser
	{
		protected enum Token
		{
			Null = 0,
			PropertyReference,
			
			CompareStart,
			GreaterThan,
			LessThan,
			GreaterThanOrEqual,
			LessThanOrEqual,
			Equals,
			NotEquals,
			CompareEnd,

			Add,
			Subtract,
			Multiply,
			Divide,
			StringLiteral,
			IntegerLiteral,
			RealLiteral,
			LeftParen,
			RightParen,
			EndOfFile
		}

		public class TokenizerException
			: Exception
		{
			public TokenizerException(string message)
				: base(message)
			{
			}
		}

		public class ParserException
			: Exception
		{
			public ParserException(string message)
				: base(message)
			{
			}
		}

		private double realValue;
		private int integerValue;
		private string stringValue;
		private string propertyReferenceValue;

		private Token token;
		private int currentIndex;
		private readonly string stringExpression;
		private readonly Expression propertyValidationContextExpression;
		
		public StringExpressionParser(Expression propertyValidationContextExpression, string stringValue)
		{
			this.stringValue = null;
			this.currentIndex = 0;
			this.stringExpression = stringValue;
			this.propertyValidationContextExpression = propertyValidationContextExpression;
		}

		public static Expression Parse(Expression propertyValidationContextExpression, string stringValue)
		{
			return new StringExpressionParser(propertyValidationContextExpression, stringValue).Parse();
		}

		private void Consume()
		{
			if (currentIndex >= stringExpression.Length)
			{
				token = Token.EndOfFile;

				return;
			}

			char currentChar = stringExpression[currentIndex];
			
			while (currentChar == ' ')
			{
				currentIndex++;

				if (currentIndex >= stringExpression.Length)
				{
					token = Token.EndOfFile;
					return;
				}

				currentChar = stringExpression[currentIndex];
			}

			if (currentChar == '{')
			{
				// Parse PropertyReference
				var propertyName = new StringBuilder();

				currentIndex++;
				currentChar = stringExpression[currentIndex];

				while (currentChar != '}')
				{
					propertyName.Append(currentChar);
					currentIndex++;
					currentChar = stringExpression[currentIndex];
				}

				currentIndex++;

				token = Token.PropertyReference;
				propertyReferenceValue = propertyName.ToString();
			}
			else if (currentChar >= '0' && currentChar <= '9')
			{
				bool isReal = false;
				var numericString = new StringBuilder();

				while (currentChar >= '0' && currentChar <= '9' || currentChar == '.')
				{
					if (currentChar == '.')
					{
						isReal = true;
					}

					numericString.Append(currentChar);
					currentIndex++;

					if (currentIndex >= stringExpression.Length)
					{
						break;
					}

					currentChar = stringExpression[currentIndex];
				}

				if (isReal)
				{
					realValue = Double.Parse(numericString.ToString());
				}
				else
				{
					integerValue = Int32.Parse(numericString.ToString());
				}

				token = Token.IntegerLiteral;
			}
			else if (currentChar == '>')
			{
				currentIndex++;

				if (stringExpression[currentIndex] == '=')
				{
					currentIndex++;

					token = Token.GreaterThanOrEqual;
				}
				else
				{
					token = Token.GreaterThan;
				}
			}
			else if (currentChar == '<')
			{
				currentIndex++;

				if (stringExpression[currentIndex] == '=')
				{
					currentIndex++;

					token = Token.LessThanOrEqual;
				}
				else
				{
					token = Token.LessThan;
				}
			}
			else if(currentChar == '+')
			{
				currentIndex++;
				token = Token.Add;
			}
			else if (currentChar == '-')
			{
				currentIndex++;
				token = Token.Subtract;
			}
			else if (currentChar == '*')
			{
				currentIndex++;
				token = Token.Multiply;
			}
			else if (currentChar == '/')
			{
				currentIndex++;
				token = Token.Divide;
			}
			else if (currentChar == '(')
			{
				currentIndex++;
				token = Token.LeftParen;
			}
			else if (currentChar == ')')
			{
				currentIndex++;
				token = Token.RightParen;
			}
			else if (currentChar == '=')
			{
				currentIndex++;
				currentChar = stringExpression[currentIndex];

				if (currentChar != '=')
				{
					throw new TokenizerException("Unknown token: =" + currentChar);
				}

				token = Token.Equals;
			}
			else
			{
				throw new TokenizerException("Unepected char: " + currentChar);
			}
		}

		protected void Expect(Token tokenValue)
		{
			if (this.token != tokenValue)
			{
				throw new ParserException("Expected token: " + tokenValue);
			}
		}

		public virtual Expression Parse()
		{
			Consume();

			return ParseExpression();
		}

		protected virtual Expression ParseExpression()
		{
			return ParseComparison();
		}

		protected Expression ParseUnary()
		{
			if (token == Token.Subtract)
			{
				Consume();

				return Expression.Multiply(Expression.Constant(-1), ParseOperand());
			}

			return ParseOperand();
		}

		protected Expression ParseComparison()
		{
			var leftOperand = ParseAddOrSubtract();
			var retval = leftOperand;

			while (token >= Token.CompareStart && token <= Token.CompareEnd)
			{
				var operationToken = token;

				Consume();

				var rightOperand = ParseAddOrSubtract();

				switch (operationToken)
				{
					case Token.Equals:
						retval = Expression.Equal(leftOperand, rightOperand);
						break;
					case Token.NotEquals:
						retval = Expression.NotEqual(leftOperand, rightOperand);
						break;
					case Token.GreaterThan:
						retval = Expression.GreaterThan(leftOperand, rightOperand);
						break;
					case Token.GreaterThanOrEqual:
						retval = Expression.GreaterThanOrEqual(leftOperand, rightOperand);
						break;
					case Token.LessThan:
						retval = Expression.LessThan(leftOperand, rightOperand);
						break;
					case Token.LessThanOrEqual:
						retval = Expression.LessThanOrEqual(leftOperand, rightOperand);
						break;
				}

				Consume();
			}

			return retval;
		}

		protected Expression ParseAddOrSubtract()
		{
			var leftOperand = ParseMultiplyOrDivide();
			var retval = leftOperand;

			while (token == Token.Add || token == Token.Subtract)
			{
				var operationToken = token;

				Consume();

				var rightOperand = ParseMultiplyOrDivide();

				if (operationToken == Token.Add)
				{
					retval = Expression.Add(leftOperand, rightOperand);
				}
				else
				{
					retval = Expression.Subtract(leftOperand, rightOperand);
				}

				Consume();
			}

			return retval;
		}

		protected Expression ParseMultiplyOrDivide()
		{
			var leftOperand = ParseUnary();
			var retval = leftOperand;

			while (token == Token.Multiply || token == Token.Divide)
			{
				var operationToken = token;

				Consume();

				var rightOperand = ParseUnary();

				if (operationToken == Token.Multiply)
				{
					retval = Expression.Multiply(leftOperand, rightOperand);
				}
				else
				{
					retval = Expression.Divide(leftOperand, rightOperand);
				}
			}

			return retval;
		}

		protected Expression ParseOperand()
		{
			if (this.token == Token.LeftParen)
			{
				Consume();

				var retval = ParseExpression();

				Consume();

				Expect(Token.RightParen);

				return retval;
			}

			switch (this.token)
			{
				case Token.PropertyReference:
					if (propertyReferenceValue == "value")
					{
						Consume();
						return Expression.Property(propertyValidationContextExpression, "PropertyValue");
					}
					else
					{
						Consume();
						return Expression.Property(Expression.Property(propertyValidationContextExpression, "ObjectValue"), propertyReferenceValue);
					}
				case Token.IntegerLiteral:
					Consume();
					return Expression.Constant(integerValue);
				case Token.RealLiteral:
					Consume();
					return Expression.Constant(realValue);
				case Token.StringLiteral:
					Consume();
					return Expression.Constant(stringValue);
				default:
					throw new ParserException("Unexpected token: " + token);
			}
		}
	}
}