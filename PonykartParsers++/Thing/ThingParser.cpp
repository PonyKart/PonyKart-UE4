#include <sstream>
#include "Thing/ThingParser.h"

using namespace std;
using namespace PonykartParsers;
using namespace ThingParser;

namespace PonykartParsers
{
namespace ThingParser
{

RuleInstance* Parser::parse(const std::string& Source)
{
	source = Source;
	index = 0;
	length = Source.size();
	laOffset = 0;
	currLine = 1;
	currChar = 1;
	RuleInstance* r;
	try
	{
		r = matchStart();
	}
	catch (...)
	{
	}
	source = string();
	fetchedTokens.clear();
	laSuccess = stack<bool>();
	laOffsets = stack<int>();
	onceOrMoreB = stack<bool>();
	return r;
}

RuleInstance* Parser::matchStart()
{
	vector<Node*> nodes;
	Token* tok;

	while (true)
	{
		if ((tok = fetchToken(laOffset))->type==NodeType::Tok_KeyBillboard || tok->type==NodeType::Tok_KeyBillboardSet
			|| tok->type==NodeType::Tok_KeyFalse || tok->type==NodeType::Tok_KeyModel
			|| tok->type==NodeType::Tok_KeyRibbon || tok->type==NodeType::Tok_KeyShape
			|| tok->type==NodeType::Tok_KeySound || tok->type==NodeType::Tok_KeyTrue || tok->type==NodeType::Tok_Name)
		{
			laOffsets.push(laOffset);
			laSuccess.push(true);
			lookaheadAnyName();
			if (laSuccess.top()) {
				if (fetchToken(laOffset)->type == NodeType::Tok_Assign)
					laOffset++;
				else
				{
					laSuccess.pop();
					laSuccess.push(false);
				}
			}
			laOffset = laOffsets.top(); laOffsets.pop();
			if (laSuccess.top())
			{
				laSuccess.pop();
				nodes.push_back(matchProperty());
			}
			else
			{
				laSuccess.pop();
				if (fetchToken(laOffset)->type == NodeType::Tok_KeyShape)
					nodes.push_back(matchShape());
				else
				{
					if (fetchToken(laOffset)->type == NodeType::Tok_KeyModel)
						nodes.push_back(matchModel());
					else
					{
						if (fetchToken(laOffset)->type == NodeType::Tok_KeyRibbon)
							nodes.push_back(matchRibbon());
						else
						{
							if (fetchToken(laOffset)->type == NodeType::Tok_KeyBillboardSet)
								nodes.push_back(matchBillboardSet());
							else
								nodes.push_back(matchSound());
						}
					}
				}
			}
		}
		else
			break;
	}
	if ((tok = nextToken())->type != NodeType::Tok_EOF)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected EOF token";
		throw string(ss.str());
	}
	nodes.push_back(tok);

	return new RuleInstance(NodeType::Rule_Start, nodes);
}

Token* Parser::fetchToken(unsigned offset)
{
	while (fetchedTokens.size() <= offset)
	{
		Token* tok = nextToken(false);
		fetchedTokens.push_back(tok);
		//if token stream is exhausted, return EOF
		if (tok->type == NodeType::Tok_EOF)
		{
			offset = fetchedTokens.size() - 1;
			break;
		}
	}
	return fetchedTokens[offset];
}

void Parser::lookaheadAnyName()
{
	if (fetchToken(laOffset)->type == NodeType::Tok_KeyFalse)
		laOffset++;
	else
	{
		if (fetchToken(laOffset)->type == NodeType::Tok_KeyModel)
			laOffset++;
		else
		{
			if (fetchToken(laOffset)->type == NodeType::Tok_KeyShape)
				laOffset++;
			else
			{
				if (fetchToken(laOffset)->type == NodeType::Tok_KeyRibbon)
					laOffset++;
				else
				{
					if (fetchToken(laOffset)->type == NodeType::Tok_KeyBillboard)
						laOffset++;
					else
					{
						if (fetchToken(laOffset)->type == NodeType::Tok_KeyBillboardSet)
							laOffset++;
						else
						{
							if (fetchToken(laOffset)->type == NodeType::Tok_KeySound)
								laOffset++;
							else
							{
								if (fetchToken(laOffset)->type == NodeType::Tok_KeyTrue)
									laOffset++;
								else
								{
									if (fetchToken(laOffset)->type == NodeType::Tok_Name)
										laOffset++;
									else
									{
										laSuccess.pop();
										laSuccess.push(false);
									}
								}
							}
						}
					}
				}
			}
		}
	}
}

Token* Parser::nextToken()
{
	return nextToken(true);
}

Token* Parser::nextToken(bool useFetched)
{
	if (useFetched && fetchedTokens.size() != 0)
	{
		Token* tok = fetchedTokens[0];
		fetchedTokens.erase(fetchedTokens.begin());
		return tok;
	}

	vector<Token*> fillers = getFillerTokens();

	if (index == length)
		return new Token(fillers, NodeType::Tok_EOF, "", currLine, currChar);

	indices = stack<int>();
	currLines = stack<int>();
	currChars = stack<int>();
	char c;
	bool pass;
	int oldIndex = index, lastLine = currLine, lastChar = currChar;

	//Token "Assign"
	pass = true;
	if (index < length && source[index] == '=')
	{
		index++;
		currChar++;
	}
	else
		pass = false;
	if (pass)
		return new Token(fillers, NodeType::Tok_Assign, source.substr(oldIndex, index - oldIndex), lastLine, lastChar);

	//Token "Comma"
	pass = true;
	if (index < length && source[index] == ',')
	{
		index++;
		currChar++;
	}
	else
		pass = false;
	if (pass)
		return new Token(fillers, NodeType::Tok_Comma, source.substr(oldIndex, index - oldIndex), lastLine, lastChar);

	//Token "LBrace"
	pass = true;
	if (index < length && source[index] == '{')
	{
		index++;
		currChar++;
	}
	else
		pass = false;
	if (pass)
		return new Token(fillers, NodeType::Tok_LBrace, source.substr(oldIndex, index - oldIndex), lastLine, lastChar);

	//Token "RBrace"
	pass = true;
	if (index < length && source[index] == '}')
	{
		index++;
		currChar++;
	}
	else
		pass = false;
	if (pass)
		return new Token(fillers, NodeType::Tok_RBrace, source.substr(oldIndex, index - oldIndex), lastLine, lastChar);

	//Token "Name"
	pass = true;
	indices.push(index);
	currLines.push(currLine);
	currChars.push(currChar);
	indices.push(index);
	currLines.push(currLine);
	currChars.push(currChar);
	if (index < length && source[index] == '@')
	{
		index++;
		currChar++;
	}
	else
		pass = false;
	if (pass)
	{
		indices.pop();
		currLines.pop();
		currChars.pop();
	}
	else
	{
		pass = true;
		index = indices.top(); indices.pop();
		currLine = currLines.top(); currLines.pop();
		currChar = currChars.top(); currChars.pop();
	}
	if (pass)
	{
		if (index < length && (c = source[index]) >= 'A' && c <= 'Z')
		{
			index++;
			currChar++;
		}
		else
			pass = false;
		if (!pass)
		{
			pass = true;
			if (index < length && (c = source[index]) >= 'a' && c <= 'z')
			{
				index++;
				currChar++;
			}
			else
				pass = false;
			if (!pass)
			{
				pass = true;
				if (index < length && source[index] == '_')
				{
					index++;
					currChar++;
				}
				else
					pass = false;
			}
		}
		if (pass)
		{
			while (true)
			{
				indices.push(index);
				currLines.push(currLine);
				currChars.push(currChar);
				if (index < length && (c = source[index]) >= 'A' && c <= 'Z')
				{
					index++;
					currChar++;
				}
				else
					pass = false;
				if (!pass)
				{
					pass = true;
					if (index < length && (c = source[index]) >= 'a' && c <= 'z')
					{
						index++;
						currChar++;
					}
					else
						pass = false;
					if (!pass)
					{
						pass = true;
						if (index < length && source[index] == '_')
						{
							index++;
							currChar++;
						}
						else
							pass = false;
					}
				}
				if (!pass)
				{
					pass = true;
					if (index < length && (c = source[index]) >= '0' && c <= '9')
					{
						index++;
						currChar++;
					}
					else
						pass = false;
				}
				if (pass)
				{
					indices.pop();
					currLines.pop();
					currChars.pop();
				}
				else
				{
					pass = true;
					index = indices.top(); indices.pop();
					currLine = currLines.top(); currLines.pop();
					currChar = currChars.top(); currChars.pop();
					break;
				}
			}
		}
	}
	if (pass)
	{
		indices.pop();
		currLines.pop();
		currChars.pop();
		return new Token(fillers, NodeType::Tok_Name, source.substr(oldIndex, index - oldIndex), lastLine, lastChar);
	}
	else
	{
		index = indices.top(); indices.pop();
		currLine = currLines.top(); currLines.pop();
		currChar = currChars.top(); currChars.pop();
	}

	//Token "StringLiteral"
	pass = true;
	indices.push(index);
	currLines.push(currLine);
	currChars.push(currChar);
	if (index < length && source[index] == '\"')
	{
		index++;
		currChar++;
	}
	else
		pass = false;
	if (pass)
	{
		while (true)
		{
			indices.push(index);
			currLines.push(currLine);
			currChars.push(currChar);
			indices.push(index);
			currLines.push(currLine);
			currChars.push(currChar);
			if (index < length && source[index] == '\\')
			{
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
			{
				if (index < length && ((c = source[index]) == 'r' || c == 'n' || c == 't' || c == '\"' || c == '\\'))
				{
					index++;
					currChar++;
				}
				else
					pass = false;
			}
			if (!pass)
			{
				pass = true;
				index = indices.top();
				currLine = currLines.top();
				currChar = currChars.top();
				if (index < length && (c = source[index]) != '\"' && c != '\\')
				{
					index++;
					if (c == '\r') {}
					else
						if (c == '\n')
						{
							currLine++;
							currChar = 1;
						}
						else
							currChar++;
				}
				else
					pass = false;
			}
			indices.pop();
			currLines.pop();
			currChars.pop();
			if (pass)
			{
				indices.pop();
				currLines.pop();
				currChars.pop();
			}
			else
			{
				pass = true;
				index = indices.top(); indices.pop();
				currLine = currLines.top(); currLines.pop();
				currChar = currChars.top(); currChars.pop();
				break;
			}
		}
		if (pass)
		{
			if (index < length && source[index] == '\"')
			{
				index++;
				currChar++;
			}
			else
				pass = false;
		}
	}
	if (pass)
	{
		indices.pop();
		currLines.pop();
		currChars.pop();
		return new Token(fillers, NodeType::Tok_StringLiteral, source.substr(oldIndex, index - oldIndex), lastLine, lastChar);
	}
	else
	{
		index = indices.top(); indices.pop();
		currLine = currLines.top(); currLines.pop();
		currChar = currChars.top(); currChars.pop();
	}

	//Token "FloatLiteral"
	pass = true;
	indices.push(index);
	currLines.push(currLine);
	currChars.push(currChar);
	indices.push(index);
	currLines.push(currLine);
	currChars.push(currChar);
	if (index < length && ((c = source[index]) == '+' || c == '-'))
	{
		index++;
		currChar++;
	}
	else
		pass = false;
	if (pass)
	{
		indices.pop();
		currLines.pop();
		currChars.pop();
	}
	else
	{
		pass = true;
		index = indices.top(); indices.pop();
		currLine = currLines.top(); currLines.pop();
		currChar = currChars.top(); currChars.pop();
	}
	if (pass)
	{
		indices.push(index);
		currLines.push(currLine);
		currChars.push(currChar);
		indices.push(index);
		currLines.push(currLine);
		currChars.push(currChar);
		indices.push(index);
		currLines.push(currLine);
		currChars.push(currChar);
		onceOrMoreB.push(false);
		while (true)
		{
			indices.push(index);
			currLines.push(currLine);
			currChars.push(currChar);
			if (index < length && (c = source[index]) >= '0' && c <= '9')
			{
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
			{
				onceOrMoreB.pop();
				onceOrMoreB.push(true);
				indices.pop();
				currLines.pop();
				currChars.pop();
			}
			else
			{
				index = indices.top(); indices.pop();
				currLine = currLines.top(); currLines.pop();
				currChar = currChars.top(); currChars.pop();
				break;
			}
		}
		pass = onceOrMoreB.top(); onceOrMoreB.pop();
		if (pass)
		{
			indices.pop();
			currLines.pop();
			currChars.pop();
		}
		else
		{
			index = indices.top(); indices.pop();
			currLine = currLines.top(); currLines.pop();
			currChar = currChars.top(); currChars.pop();
		}
		if (pass)
		{
			if (index < length && source[index] == '.')
			{
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
			{
				while (true)
				{
					indices.push(index);
					currLines.push(currLine);
					currChars.push(currChar);
					if (index < length && (c = source[index]) >= '0' && c <= '9')
					{
						index++;
						currChar++;
					}
					else
						pass = false;
					if (pass)
					{
						indices.pop();
						currLines.pop();
						currChars.pop();
					}
					else
					{
						pass = true;
						index = indices.top(); indices.pop();
						currLine = currLines.top(); currLines.pop();
						currChar = currChars.top(); currChars.pop();
						break;
					}
				}
			}
		}
		if (!pass)
		{
			pass = true;
			index = indices.top();
			currLine = currLines.top();
			currChar = currChars.top();
			if (index < length && source[index] == '.')
			{
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
			{
				indices.push(index);
				currLines.push(currLine);
				currChars.push(currChar);
				onceOrMoreB.push(false);
				while (true)
				{
					indices.push(index);
					currLines.push(currLine);
					currChars.push(currChar);
					if (index < length && (c = source[index]) >= '0' && c <= '9')
					{
						index++;
						currChar++;
					}
					else
						pass = false;
					if (pass)
					{
						onceOrMoreB.pop();
						onceOrMoreB.push(true);
						indices.pop();
						currLines.pop();
						currChars.pop();
					}
					else
					{
						index = indices.top(); indices.pop();
						currLine = currLines.top(); currLines.pop();
						currChar = currChars.top(); currChars.pop();
						break;
					}
				}
				pass = onceOrMoreB.top(); onceOrMoreB.pop();
				if (pass)
				{
					indices.pop();
					currLines.pop();
					currChars.pop();
				}
				else
				{
					index = indices.top(); indices.pop();
					currLine = currLines.top(); currLines.pop();
					currChar = currChars.top(); currChars.pop();
				}
			}
		}
		indices.pop();
		currLines.pop();
		currChars.pop();
		if (pass)
		{
			indices.push(index);
			currLines.push(currLine);
			currChars.push(currChar);
			if (index < length && ((c = source[index]) == 'E' || c == 'e'))
			{
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
			{
				indices.push(index);
				currLines.push(currLine);
				currChars.push(currChar);
				if (index < length && ((c = source[index]) == '+' || c == '-'))
				{
					index++;
					currChar++;
				}
				else
					pass = false;
				if (pass)
				{
					indices.pop();
					currLines.pop();
					currChars.pop();
				}
				else
				{
					pass = true;
					index = indices.top(); indices.pop();
					currLine = currLines.top(); currLines.pop();
					currChar = currChars.top(); currChars.pop();
				}
				if (pass)
				{
					indices.push(index);
					currLines.push(currLine);
					currChars.push(currChar);
					onceOrMoreB.push(false);
					while (true)
					{
						indices.push(index);
						currLines.push(currLine);
						currChars.push(currChar);
						if (index < length && (c = source[index]) >= '0' && c <= '9')
						{
							index++;
							currChar++;
						}
						else
							pass = false;
						if (pass)
						{
							onceOrMoreB.pop();
							onceOrMoreB.push(true);
							indices.pop();
							currLines.pop();
							currChars.pop();
						}
						else
						{
							index = indices.top(); indices.pop();
							currLine = currLines.top(); currLines.pop();
							currChar = currChars.top(); currChars.pop();
							break;
						}
					}
					pass = onceOrMoreB.top(); onceOrMoreB.pop();
					if (pass)
					{
						indices.pop();
						currLines.pop();
						currChars.pop();
					}
					else
					{
						index = indices.top(); indices.pop();
						currLine = currLines.top(); currLines.pop();
						currChar = currChars.top(); currChars.pop();
					}
				}
			}
			if (pass)
			{
				indices.pop();
				currLines.pop();
				currChars.pop();
			}
			else
			{
				pass = true;
				index = indices.top(); indices.pop();
				currLine = currLines.top(); currLines.pop();
				currChar = currChars.top(); currChars.pop();
			}
		}
		if (!pass)
		{
			pass = true;
			index = indices.top();
			currLine = currLines.top();
			currChar = currChars.top();
			if (index < length && source[index] == '0')
			{
				index++;
				currChar++;
			}
			else
				pass = false;
			if (!pass)
			{
				pass = true;
				if (index < length && (c = source[index]) >= '1' && c <= '9')
				{
					index++;
					currChar++;
				}
				else
					pass = false;
				if (pass)
				{
					while (true)
					{
						indices.push(index);
						currLines.push(currLine);
						currChars.push(currChar);
						if (index < length && (c = source[index]) >= '0' && c <= '9')
						{
							index++;
							currChar++;
						}
						else
							pass = false;
						if (pass)
						{
							indices.pop();
							currLines.pop();
							currChars.pop();
						}
						else
						{
							pass = true;
							index = indices.top(); indices.pop();
							currLine = currLines.top(); currLines.pop();
							currChar = currChars.top(); currChars.pop();
							break;
						}
					}
				}
			}
			if (pass)
			{
				if (index < length && ((c = source[index]) == 'E' || c == 'e'))
				{
					index++;
					currChar++;
				}
				else
					pass = false;
				if (pass)
				{
					indices.push(index);
					currLines.push(currLine);
					currChars.push(currChar);
					if (index < length && ((c = source[index]) == '+' || c == '-'))
					{
						index++;
						currChar++;
					}
					else
						pass = false;
					if (pass)
					{
						indices.pop();
						currLines.pop();
						currChars.pop();
					}
					else
					{
						pass = true;
						index = indices.top(); indices.pop();
						currLine = currLines.top(); currLines.pop();
						currChar = currChars.top(); currChars.pop();
					}
					if (pass)
					{
						indices.push(index);
						currLines.push(currLine);
						currChars.push(currChar);
						onceOrMoreB.push(false);
						while (true)
						{
							indices.push(index);
							currLines.push(currLine);
							currChars.push(currChar);
							if (index < length && (c = source[index]) >= '0' && c <= '9')
							{
								index++;
								currChar++;
							}
							else
								pass = false;
							if (pass)
							{
								onceOrMoreB.pop();
								onceOrMoreB.push(true);
								indices.pop();
								currLines.pop();
								currChars.pop();
							}
							else
							{
								index = indices.top(); indices.pop();
								currLine = currLines.top(); currLines.pop();
								currChar = currChars.top(); currChars.pop();
								break;
							}
						}
						pass = onceOrMoreB.top(); onceOrMoreB.pop();
						if (pass)
						{
							indices.pop();
							currLines.pop();
							currChars.pop();
						}
						else
						{
							index = indices.top(); indices.pop();
							currLine = currLines.top(); currLines.pop();
							currChar = currChars.top(); currChars.pop();
						}
					}
				}
			}
		}
		indices.pop();
		currLines.pop();
		currChars.pop();
	}
	if (pass)
	{
		indices.pop();
		currLines.pop();
		currChars.pop();
		return new Token(fillers, NodeType::Tok_FloatLiteral, source.substr(oldIndex, index - oldIndex), lastLine, lastChar);
	}
	else
	{
		index = indices.top(); indices.pop();
		currLine = currLines.top(); currLines.pop();
		currChar = currChars.top(); currChars.pop();
	}

	//Token "IntLiteral"
	pass = true;
	indices.push(index);
	currLines.push(currLine);
	currChars.push(currChar);
	if (index < length && source[index] == '0')
	{
		index++;
		currChar++;
	}
	else
		pass = false;
	if (pass)
	{
		if (index < length && ((c = source[index]) == 'X' || c == 'x'))
		{
			index++;
			currChar++;
		}
		else
			pass = false;
		if (pass)
		{
			indices.push(index);
			currLines.push(currLine);
			currChars.push(currChar);
			onceOrMoreB.push(false);
			while (true)
			{
				indices.push(index);
				currLines.push(currLine);
				currChars.push(currChar);
				if (index < length && (c = source[index]) >= '0' && c <= '9')
				{
					index++;
					currChar++;
				}
				else
					pass = false;
				if (!pass)
				{
					pass = true;
					if (index < length && (c = source[index]) >= 'A' && c <= 'F')
					{
						index++;
						currChar++;
					}
					else
						pass = false;
					if (!pass)
					{
						pass = true;
						if (index < length && (c = source[index]) >= 'a' && c <= 'f')
						{
							index++;
							currChar++;
						}
						else
							pass = false;
					}
				}
				if (pass)
				{
					onceOrMoreB.pop();
					onceOrMoreB.push(true);
					indices.pop();
					currLines.pop();
					currChars.pop();
				}
				else
				{
					index = indices.top(); indices.pop();
					currLine = currLines.top(); currLines.pop();
					currChar = currChars.top(); currChars.pop();
					break;
				}
			}
			pass = onceOrMoreB.top(); onceOrMoreB.pop();
			if (pass)
			{
				indices.pop();
				currLines.pop();
				currChars.pop();
			}
			else
			{
				index = indices.top(); indices.pop();
				currLine = currLines.top(); currLines.pop();
				currChar = currChars.top(); currChars.pop();
			}
		}
	}
	if (!pass)
	{
		pass = true;
		index = indices.top();
		currLine = currLines.top();
		currChar = currChars.top();
		indices.push(index);
		currLines.push(currLine);
		currChars.push(currChar);
		if (index < length && ((c = source[index]) == '+' || c == '-'))
		{
			index++;
			currChar++;
		}
		else
			pass = false;
		if (pass)
		{
			indices.pop();
			currLines.pop();
			currChars.pop();
		}
		else
		{
			pass = true;
			index = indices.top(); indices.pop();
			currLine = currLines.top(); currLines.pop();
			currChar = currChars.top(); currChars.pop();
		}
		if (pass)
		{
			if (index < length && source[index] == '0')
			{
				index++;
				currChar++;
			}
			else
				pass = false;
			if (!pass)
			{
				pass = true;
				if (index < length && (c = source[index]) >= '1' && c <= '9')
				{
					index++;
					currChar++;
				}
				else
					pass = false;
				if (pass)
				{
					while (true)
					{
						indices.push(index);
						currLines.push(currLine);
						currChars.push(currChar);
						if (index < length && (c = source[index]) >= '0' && c <= '9')
						{
							index++;
							currChar++;
						}
						else
							pass = false;
						if (pass)
						{
							indices.pop();
							currLines.pop();
							currChars.pop();
						}
						else
						{
							pass = true;
							index = indices.top(); indices.pop();
							currLine = currLines.top(); currLines.pop();
							currChar = currChars.top(); currChars.pop();
							break;
						}
					}
				}
			}
		}
	}
	indices.pop();
	currLines.pop();
	currChars.pop();
	if (pass)
		return new Token(fillers, NodeType::Tok_IntLiteral, source.substr(oldIndex, index - oldIndex), lastLine, lastChar);

	stringstream ss; ss << "Line " << currLine << ", char " << currChar << ": No token match";
	throw string(ss.str());
}

RuleInstance* Parser::matchProperty()
{
	vector<Node*> nodes;

	laOffsets.push(laOffset);
	laSuccess.push(true);
	lookaheadBoolProperty();
	laOffset = laOffsets.top(); laOffsets.pop();
	if (laSuccess.top())
	{
		laSuccess.pop();
		nodes.push_back(matchBoolProperty());
	}
	else
	{
		laSuccess.pop();
		laOffsets.push(laOffset);
		laSuccess.push(true);
		lookaheadEnumProperty();
		laOffset = laOffsets.top(); laOffsets.pop();
		if (laSuccess.top())
		{
			laSuccess.pop();
			nodes.push_back(matchEnumProperty());
		}
		else
		{
			laSuccess.pop();
			laOffsets.push(laOffset);
			laSuccess.push(true);
			lookaheadAnyName();
			if (laSuccess.top())
			{
				if (fetchToken(laOffset)->type == NodeType::Tok_Assign)
					laOffset++;
				else
				{
					laSuccess.pop();
					laSuccess.push(false);
				}
			}
			if (laSuccess.top())
			{
				if (fetchToken(laOffset)->type == NodeType::Tok_IntLiteral)
				{
					if (fetchToken(laOffset)->type == NodeType::Tok_IntLiteral)
						laOffset++;
					else
					{
						laSuccess.pop();
						laSuccess.push(false);
					}
				}
				else
				{
					if (fetchToken(laOffset)->type == NodeType::Tok_FloatLiteral)
						laOffset++;
					else
					{
						laSuccess.pop();
						laSuccess.push(false);
					}
				}
			}
			if (laSuccess.top())
			{
				if (fetchToken(laOffset)->type == NodeType::Tok_Comma)
					laOffset++;
				else
				{
					laSuccess.pop();
					laSuccess.push(false);
				}
			}
			if (laSuccess.top())
			{
				if (fetchToken(laOffset)->type == NodeType::Tok_IntLiteral)
				{
					if (fetchToken(laOffset)->type == NodeType::Tok_IntLiteral)
						laOffset++;
					else
					{
						laSuccess.pop();
						laSuccess.push(false);
					}
				}
				else
				{
					if (fetchToken(laOffset)->type == NodeType::Tok_FloatLiteral)
						laOffset++;
					else
					{
						laSuccess.pop();
						laSuccess.push(false);
					}
				}
			}
			if (laSuccess.top())
			{
				if (fetchToken(laOffset)->type == NodeType::Tok_Comma)
					laOffset++;
				else
				{
					laSuccess.pop();
					laSuccess.push(false);
				}
			}
			if (laSuccess.top())
			{
				if (fetchToken(laOffset)->type == NodeType::Tok_IntLiteral)
				{
					if (fetchToken(laOffset)->type == NodeType::Tok_IntLiteral)
						laOffset++;
					else
					{
						laSuccess.pop();
						laSuccess.push(false);
					}
				}
				else
				{
					if (fetchToken(laOffset)->type == NodeType::Tok_FloatLiteral)
						laOffset++;
					else
					{
						laSuccess.pop();
						laSuccess.push(false);
					}
				}
			}
			if (laSuccess.top())
			{
				if (fetchToken(laOffset)->type == NodeType::Tok_Comma)
					laOffset++;
				else
				{
					laSuccess.pop();
					laSuccess.push(false);
				}
			}
			laOffset = laOffsets.top(); laOffsets.pop();
			if (laSuccess.top())
			{
				laSuccess.pop();
				nodes.push_back(matchQuatProperty());
			}
			else
			{
				laSuccess.pop();
				laOffsets.push(laOffset);
				laSuccess.push(true);
				lookaheadAnyName();
				if (laSuccess.top())
				{
					if (fetchToken(laOffset)->type == NodeType::Tok_Assign)
						laOffset++;
					else
					{
						laSuccess.pop();
						laSuccess.push(false);
					}
				}
				if (laSuccess.top())
				{
					if (fetchToken(laOffset)->type == NodeType::Tok_IntLiteral)
					{
						if (fetchToken(laOffset)->type == NodeType::Tok_IntLiteral)
							laOffset++;
						else
						{
							laSuccess.pop();
							laSuccess.push(false);
						}
					}
					else
					{
						if (fetchToken(laOffset)->type == NodeType::Tok_FloatLiteral)
							laOffset++;
						else
						{
							laSuccess.pop();
							laSuccess.push(false);
						}
					}
				}
				if (laSuccess.top())
				{
					if (fetchToken(laOffset)->type == NodeType::Tok_Comma)
						laOffset++;
					else
					{
						laSuccess.pop();
						laSuccess.push(false);
					}
				}
				laOffset = laOffsets.top(); laOffsets.pop();
				if (laSuccess.top())
				{
					laSuccess.pop();
					nodes.push_back(matchVec3Property());
				}
				else
				{
					laSuccess.pop();
					laOffsets.push(laOffset);
					laSuccess.push(true);
					lookaheadNumericProperty();
					laOffset = laOffsets.top(); laOffsets.pop();
					if (laSuccess.top())
					{
						laSuccess.pop();
						nodes.push_back(matchNumericProperty());
					}
					else
					{
						laSuccess.pop();
						nodes.push_back(matchStringProperty());
					}
				}
			}
		}
	}

	return new RuleInstance(NodeType::Rule_Property, nodes);
}

RuleInstance* Parser::matchShape()
{
	vector<Node*> nodes;
	Token* tok;

	if ((tok = nextToken())->type != NodeType::Tok_KeyShape)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected KeyShape token";
		throw string(ss.str());
	}
	nodes.push_back(tok);
	if ((tok = nextToken())->type != NodeType::Tok_LBrace)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected LBrace token";
		throw string(ss.str());
	}
	nodes.push_back(tok);
	while (true) {
		if ((tok = fetchToken(laOffset))->type == NodeType::Tok_KeyBillboard || tok->type == NodeType::Tok_KeyBillboardSet || tok->type == NodeType::Tok_KeyFalse || tok->type == NodeType::Tok_KeyModel || tok->type == NodeType::Tok_KeyRibbon || tok->type == NodeType::Tok_KeyShape || tok->type == NodeType::Tok_KeySound || tok->type == NodeType::Tok_KeyTrue || tok->type == NodeType::Tok_Name) {
			nodes.push_back(matchProperty());
		}
		else
			break;
	}
	if ((tok = nextToken())->type != NodeType::Tok_RBrace)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected RBrace token";
		throw string(ss.str());
	}
	nodes.push_back(tok);

	return new RuleInstance(NodeType::Rule_Shape, nodes);
}

RuleInstance* Parser::matchModel()
{
	vector<Node*> nodes;
	Token* tok;

	if ((tok = nextToken())->type != NodeType::Tok_KeyModel)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected KeyModel token";
		throw string(ss.str());
	}
	nodes.push_back(tok);
	if ((tok = nextToken())->type != NodeType::Tok_LBrace)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected LBrace token";
		throw string(ss.str());
	}
	nodes.push_back(tok);
	while (true)
	{
		if ((tok = fetchToken(laOffset))->type==NodeType::Tok_KeyBillboard || tok->type==NodeType::Tok_KeyBillboardSet
			|| tok->type==NodeType::Tok_KeyFalse || tok->type==NodeType::Tok_KeyModel
			|| tok->type==NodeType::Tok_KeyRibbon || tok->type==NodeType::Tok_KeyShape
			|| tok->type==NodeType::Tok_KeySound || tok->type==NodeType::Tok_KeyTrue || tok->type == NodeType::Tok_Name)
		{
			nodes.push_back(matchProperty());
		}
		else
			break;
	}
	if ((tok = nextToken())->type != NodeType::Tok_RBrace)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected RBrace token";
		throw string(ss.str());
	}
	nodes.push_back(tok);

	return new RuleInstance(NodeType::Rule_Model, nodes);
}

RuleInstance* Parser::matchRibbon()
{
	vector<Node*> nodes;
	Token* tok;

	if ((tok = nextToken())->type != NodeType::Tok_KeyRibbon)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected KeyRibbon token";
		throw string(ss.str());
	}
	nodes.push_back(tok);
	if ((tok = nextToken())->type != NodeType::Tok_LBrace)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected LBrace token";
		throw string(ss.str());
	}
	nodes.push_back(tok);
	while (true) {
		if ((tok = fetchToken(laOffset))->type == NodeType::Tok_KeyBillboard || tok->type == NodeType::Tok_KeyBillboardSet || tok->type == NodeType::Tok_KeyFalse || tok->type == NodeType::Tok_KeyModel || tok->type == NodeType::Tok_KeyRibbon || tok->type == NodeType::Tok_KeyShape || tok->type == NodeType::Tok_KeySound || tok->type == NodeType::Tok_KeyTrue || tok->type == NodeType::Tok_Name) {
			nodes.push_back(matchProperty());
		}
		else
			break;
	}
	if ((tok = nextToken())->type != NodeType::Tok_RBrace)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected RBrace token";
		throw string(ss.str());
	}
	nodes.push_back(tok);

	return new RuleInstance(NodeType::Rule_Ribbon, nodes);
}

RuleInstance* Parser::matchBillboardSet()
{
	vector<Node*> nodes;
	Token* tok;

	if ((tok = nextToken())->type != NodeType::Tok_KeyBillboardSet)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected KeyBillboardSet token";
		throw string(ss.str());
	}
	nodes.push_back(tok);
	if ((tok = nextToken())->type != NodeType::Tok_LBrace)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected LBrace token";
		throw string(ss.str());
	}
	nodes.push_back(tok);
	while (true) {
		if ((tok = fetchToken(laOffset))->type == NodeType::Tok_KeyBillboard || tok->type == NodeType::Tok_KeyBillboardSet || tok->type == NodeType::Tok_KeyFalse || tok->type == NodeType::Tok_KeyModel || tok->type == NodeType::Tok_KeyRibbon || tok->type == NodeType::Tok_KeyShape || tok->type == NodeType::Tok_KeySound || tok->type == NodeType::Tok_KeyTrue || tok->type == NodeType::Tok_Name) {
			laOffsets.push(laOffset);
			laSuccess.push(true);
			if (fetchToken(laOffset)->type == NodeType::Tok_KeyBillboard)
				laOffset++;
			else
			{
				laSuccess.pop();
				laSuccess.push(false);
			}
			laOffset = laOffsets.top(); laOffsets.pop();
			if (laSuccess.top())
			{
				laSuccess.pop();
				nodes.push_back(matchBillboard());
			}
			else
			{
				laSuccess.pop();
				nodes.push_back(matchProperty());
			}
		}
		else
			break;
	}
	if ((tok = nextToken())->type != NodeType::Tok_RBrace)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected RBrace token";
		throw string(ss.str());
	}
	nodes.push_back(tok);

	return new RuleInstance(NodeType::Rule_BillboardSet, nodes);
}

RuleInstance* Parser::matchBillboard()
{
	vector<Node*> nodes;
	Token* tok;

	if ((tok = nextToken())->type != NodeType::Tok_KeyBillboard)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected KeyBillboard token";
		throw string(ss.str());
	}
	nodes.push_back(tok);
	if ((tok = nextToken())->type != NodeType::Tok_LBrace)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected LBrace token";
		throw string(ss.str());
	}
	nodes.push_back(tok);
	while (true) {
		if ((tok = fetchToken(laOffset))->type == NodeType::Tok_KeyBillboard || tok->type == NodeType::Tok_KeyBillboardSet || tok->type == NodeType::Tok_KeyFalse || tok->type == NodeType::Tok_KeyModel || tok->type == NodeType::Tok_KeyRibbon || tok->type == NodeType::Tok_KeyShape || tok->type == NodeType::Tok_KeySound || tok->type == NodeType::Tok_KeyTrue || tok->type == NodeType::Tok_Name) {
			nodes.push_back(matchProperty());
		}
		else
			break;
	}
	if ((tok = nextToken())->type != NodeType::Tok_RBrace)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected RBrace token";
		throw string(ss.str());
	}
	nodes.push_back(tok);

	return new RuleInstance(NodeType::Rule_Billboard, nodes);
}

RuleInstance* Parser::matchSound()
{
	vector<Node*> nodes;
	Token* tok;

	if ((tok = nextToken())->type != NodeType::Tok_KeySound)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected KeySound token";
		throw string(ss.str());
	}
	nodes.push_back(tok);
	if ((tok = nextToken())->type != NodeType::Tok_LBrace)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected LBrace token";
		throw string(ss.str());
	}
	nodes.push_back(tok);
	while (true) {
		if ((tok = fetchToken(laOffset))->type == NodeType::Tok_KeyBillboard || tok->type == NodeType::Tok_KeyBillboardSet || tok->type == NodeType::Tok_KeyFalse || tok->type == NodeType::Tok_KeyModel || tok->type == NodeType::Tok_KeyRibbon || tok->type == NodeType::Tok_KeyShape || tok->type == NodeType::Tok_KeySound || tok->type == NodeType::Tok_KeyTrue || tok->type == NodeType::Tok_Name) {
			nodes.push_back(matchProperty());
		}
		else
			break;
	}
	if ((tok = nextToken())->type != NodeType::Tok_RBrace)
	{
		stringstream ss; ss << "Line " << tok->lineNr << ", char " << tok->charNr << ": Expected RBrace token";
		throw string(ss.str());
	}
	nodes.push_back(tok);

	return new RuleInstance(NodeType::Rule_Sound, nodes);
}

} // ThingParser
} // PonykartParsers
