#include <stack>
#include <cstdint>
#include <iostream>
#include <fstream>

/* Generated code */

namespace _Gen
{
#include "parantheses.inl"
}

class FullParser : public _Gen::Parser
{
  public:
    FullParser(const char *filename) : mFile(filename)
    {
    }

  protected:
    BUToken nextToken(void **param) override
    {
        *param = nullptr;
        char c = mFile.get();
        if (!mFile.good())
            return _BUT_EOF;

        std::cout << "Got token " << c << std::endl;
        switch (c)
        {
        case '(':
            return BUT_0;
        case ')':
            return BUT_1;
        case ' ':
        case '\n':
            return nextToken(param);
        default:
            std::cout << "Bad token " << c << std::endl;
            return _BUT_EOF;
        }
    }

  private:
    std::ifstream mFile;
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
        std::cout << "GOOD :)" << std::endl;
        return 0;
    }
    else
    {
        std::cout << "BAD :(" << std::endl;
        return -1;
    }
}
