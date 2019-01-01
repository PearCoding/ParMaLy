#include <vector>
#include <stack>
#include <cstdint>
#include <cmath>
#include <iostream>
#include <fstream>

/* Generated code */
enum ExpressionType
{
    ET_Num,
    ET_Add,
    ET_Sub,
    ET_Mul,
    ET_Div,
    ET_Mod,
    ET_Pow
};

struct Number
{
    double N;
};

struct Expression
{
    ExpressionType Type;
    Number *N;
    Expression *Left;
    Expression *Right;
};

Expression *Expr(ExpressionType type, Number *e)
{
    return new Expression{type, e, nullptr, nullptr};
}

Expression *Expr(ExpressionType type, Expression *left, Expression *right)
{
    return new Expression{type, nullptr, left, right};
}

float Calc(Expression *expr)
{
    if (!expr)
        return 0.0f;

    switch (expr->Type)
    {
    case ET_Num:
        return expr->N->N;
    case ET_Add:
        return Calc(expr->Left) + Calc(expr->Right);
    case ET_Sub:
        return Calc(expr->Left) - Calc(expr->Right);
    case ET_Mul:
        return Calc(expr->Left) * Calc(expr->Right);
    case ET_Div:
        return Calc(expr->Left) / Calc(expr->Right);
    case ET_Mod:
        return std::fmod(Calc(expr->Left), Calc(expr->Right));
    case ET_Pow:
        return std::pow(Calc(expr->Left), Calc(expr->Right));
    default:
        return 0.0f;
    }
}

namespace _Gen
{
#include "calc.inl"
}

class FullParser : public _Gen::Parser
{
  public:
    FullParser(const char *str) : mString(str)
    {
        mIt = mString.begin();
    }

  protected:
    _Gen::BUToken nextToken(void **param) override
    {
        *param = nullptr;
        if (mIt == mString.end())
            return _Gen::_BUT_EOF;

        char c = *mIt;
        ++mIt;

        //std::cout << "Token: " << c << std::endl;
        switch (c)
        {
        case ';':
        case '+':
        case '-':
        case '*':
        case '/':
        case '%':
        case '(':
        case ')':
            return (_Gen::BUToken)c;
        case ' ':
        case '\t':
        case '\n':
            return nextToken(param);
        default:
            if (std::isdigit(c))
            {
                std::string str = "";
                while (mIt != mString.end() && std::isdigit(*mIt))
                {
                    str += *mIt;
                    ++mIt;
                }

                Number *number = new Number();
                number->N = std::stoi(str);
                *param = number;
                return _Gen::BUT_NUM;
            }
            else
            {
                std::cout << "Bad token " << c << std::endl;
                return _Gen::_BUT_EOF;
            }
        }
    }

  private:
    std::string mString;
    std::string::const_iterator mIt;
};

int main(int argc, char **argv)
{
    if (argc < 2)
    {
        std::cout << "Not enough arguments given..." << std::endl;
        return -1;
    }

    FullParser parser(argv[1]);
    _Gen::ParseResult ret = parser.parse();
    if (ret.Successful)
    {
        std::cout << Calc((Expression *)ret.Return) << std::endl;
        return 0;
    }
    else
    {
        std::cout << "BAD :(" << std::endl;
        return -1;
    }
}
