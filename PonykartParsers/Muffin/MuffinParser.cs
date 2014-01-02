//Generated with Imperator Parser Generator v. 1.2.2-BETA @ 2011-11-07
//Imperator by Max 'Shrinker' Wieden
//http://shrinker.beyond-veils.de/projects/IterationX/Imperator/
//http://shrinker.scottbaker.eu/projects/IterationX/Imperator/
using System;
using System.Collections.Generic;

namespace PonykartParsers.MuffinParser {
	public class ParserException : Exception {
		public ParserException(string message) :
			base(message) {
		}
	}

	public enum NodeType {
		Tok_EOF,
		Tok_Assign,
		Tok_Comma,
		Tok_LBrace,
		Tok_RBrace,
		Tok_Name,
		Tok_KeyFalse,
		Tok_KeyTrue,
		Tok_StringLiteral,
		Tok_FloatLiteral,
		Tok_IntLiteral,
		Tok_SingleLineComment,
		Tok_MultiLineComment,
		Tok_Whitespace,
		Rule_Start,
		Rule_Property,
		Rule_EnumProperty,
		Rule_QuatProperty,
		Rule_Vec3Property,
		Rule_NumericProperty,
		Rule_StringProperty,
		Rule_BoolProperty,
		Rule_Block,
		Rule_AnyName
	}

	public abstract class Node {
		public static string TypeName(NodeType type) {
			string s = type.ToString();
			return s.Substring(s.IndexOf('_') + 1);
		}

		public readonly NodeType Type;

		public Node(NodeType type) {
			Type = type;
		}
	}

	///<summary>represents an inner node</summary>
	public class RuleInstance : Node {
		public readonly Node[] Children;

		public RuleInstance(NodeType type, Node[] children) :
			base(type) {
			Children = children;
		}
	}

	///<summary>represents a leaf node</summary>
	public class Token : Node {
		public readonly Token[] PrecedingFillerTokens;
		public readonly string Image;
		public readonly int LineNr, CharNr;

		private static Dictionary<string, NodeType> specForTok_Name;

		static Token() {
			specForTok_Name = new Dictionary<string, NodeType>();
			specForTok_Name.Add("false", NodeType.Tok_KeyFalse);
			specForTok_Name.Add("true", NodeType.Tok_KeyTrue);
		}

		private static NodeType specializeType(NodeType type, string image) {
			NodeType type2;
			switch (type) {
				case NodeType.Tok_Name:
					if (specForTok_Name.TryGetValue(image, out type2))
						return type2;
					if (specForTok_Name.TryGetValue(image.ToLowerInvariant(), out type2))
						return type2;
					break;
			}
			return type;
		}

		public Token(Token[] precedingFillerTokens, NodeType type, string image, int lineNr, int charNr) :
			base(specializeType(type, image)) {
			PrecedingFillerTokens = precedingFillerTokens;
			Image = image;
			LineNr = lineNr;
			CharNr = charNr;
		}
	}

	public class Parser {
		private string source;
		private int index, length, currLine, currChar, laOffset;
		private readonly List <Token> fetchedTokens;
		private readonly Stack<bool > laSuccess;
		private readonly Stack<int  > laOffsets;
		private readonly Stack<bool > onceOrMoreB;
		private readonly List<Token> tokens;
		private readonly Stack<int> indices, currLines, currChars;

		public Parser() {
			fetchedTokens = new List<Token>();
			laSuccess = new Stack<bool>();
			laOffsets = new Stack<int>();
			onceOrMoreB = new Stack<bool>();
			tokens = new List<Token>();
			indices = new Stack<int>();
			currLines = new Stack<int>();
			currChars = new Stack<int>();
		}

		public RuleInstance Parse(string source) {
			this.source = source;
			index = 0;
			length = source.Length;
			laOffset = 0;
			currLine = 1;
			currChar = 1;
			RuleInstance r;
			try {
				r = matchStart();
			}
			finally {
				this.source = null;
				fetchedTokens.Clear();
				laSuccess.Clear();
				laOffsets.Clear();
				onceOrMoreB.Clear();
			}
			return r;
		}

		private Token nextToken() {
			return nextToken(true);
		}

		private Token fetchToken(int offset) {
			while (fetchedTokens.Count <= offset) {
				Token tok = nextToken(false);
				fetchedTokens.Add(tok);
				//if token stream is exhausted, return EOF
				if (tok.Type == NodeType.Tok_EOF) {
					offset = fetchedTokens.Count - 1;
					break;
				}
			}
			return fetchedTokens[offset];
		}

		private Token[] getFillerTokens() {
			tokens.Clear();
			indices.Clear();
			currLines.Clear();
			currChars.Clear();
			char c;
			bool pass;
			while (true) {
				int oldIndex = index, lastLine = currLine, lastChar = currChar;

				//Token "SingleLineComment"
				pass = true;
				indices.Push(index);
				currLines.Push(currLine);
				currChars.Push(currChar);
				if (index + 2 < length && string.Compare(source, index, "//", 0, 2) == 0) {
					index += 2;
					currChar += 2;
				}
				else
					pass = false;
				if (pass) {
					while (true) {
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						if (index < length && (c = source[index]) != '\r' && c != '\n') {
							index++;
							currChar++;
						}
						else
							pass = false;
						if (pass) {
							indices.Pop();
							currLines.Pop();
							currChars.Pop();
						}
						else {
							pass = true;
							index = indices.Pop();
							currLine = currLines.Pop();
							currChar = currChars.Pop();
							break;
						}
					}
					if (pass) {
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						if (index + 2 < length && string.Compare(source, index, "\r\n", 0, 2) == 0) {
							index += 2;
							currLine++;
							currChar = 1;
						}
						else
							pass = false;
						if (!pass) {
							pass = true;
							if (index < length && ((c = source[index]) == '\r' || c == '\n')) {
								index++;
								if (c == '\r') {
								}
								else
									if (c == '\n') {
										currLine++;
										currChar = 1;
									}
									else
										currChar++;
							}
							else
								pass = false;
						}
						if (pass) {
							indices.Pop();
							currLines.Pop();
							currChars.Pop();
						}
						else {
							pass = true;
							index = indices.Pop();
							currLine = currLines.Pop();
							currChar = currChars.Pop();
						}
					}
				}
				if (pass) {
					indices.Pop();
					currLines.Pop();
					currChars.Pop();
					tokens.Add(new Token(null, NodeType.Tok_SingleLineComment, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar));
					continue;
				}
				else {
					index = indices.Pop();
					currLine = currLines.Pop();
					currChar = currChars.Pop();
				}

				//Token "MultiLineComment"
				pass = true;
				indices.Push(index);
				currLines.Push(currLine);
				currChars.Push(currChar);
				if (index + 2 < length && string.Compare(source, index, "/*", 0, 2) == 0) {
					index += 2;
					currChar += 2;
				}
				else
					pass = false;
				if (pass) {
					while (true) {
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						if (index < length && (c = source[index]) != '*') {
							index++;
							if (c == '\r') {
							}
							else
								if (c == '\n') {
									currLine++;
									currChar = 1;
								}
								else
									currChar++;
						}
						else
							pass = false;
						if (pass) {
							indices.Pop();
							currLines.Pop();
							currChars.Pop();
						}
						else {
							pass = true;
							index = indices.Pop();
							currLine = currLines.Pop();
							currChar = currChars.Pop();
							break;
						}
					}
					if (pass) {
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						onceOrMoreB.Push(false);
						while (true) {
							indices.Push(index);
							currLines.Push(currLine);
							currChars.Push(currChar);
							if (index < length && source[index] == '*') {
								index++;
								currChar++;
							}
							else
								pass = false;
							if (pass) {
								onceOrMoreB.Pop();
								onceOrMoreB.Push(true);
								indices.Pop();
								currLines.Pop();
								currChars.Pop();
							}
							else {
								index = indices.Pop();
								currLine = currLines.Pop();
								currChar = currChars.Pop();
								break;
							}
						}
						pass = onceOrMoreB.Pop();
						if (pass) {
							indices.Pop();
							currLines.Pop();
							currChars.Pop();
						}
						else {
							index = indices.Pop();
							currLine = currLines.Pop();
							currChar = currChars.Pop();
						}
						if (pass) {
							while (true) {
								indices.Push(index);
								currLines.Push(currLine);
								currChars.Push(currChar);
								if (index < length && (c = source[index]) != '*' && c != '/') {
									index++;
									if (c == '\r') {
									}
									else
										if (c == '\n') {
											currLine++;
											currChar = 1;
										}
										else
											currChar++;
								}
								else
									pass = false;
								if (pass) {
									while (true) {
										indices.Push(index);
										currLines.Push(currLine);
										currChars.Push(currChar);
										if (index < length && (c = source[index]) != '*') {
											index++;
											if (c == '\r') {
											}
											else
												if (c == '\n') {
													currLine++;
													currChar = 1;
												}
												else
													currChar++;
										}
										else
											pass = false;
										if (pass) {
											indices.Pop();
											currLines.Pop();
											currChars.Pop();
										}
										else {
											pass = true;
											index = indices.Pop();
											currLine = currLines.Pop();
											currChar = currChars.Pop();
											break;
										}
									}
									if (pass) {
										indices.Push(index);
										currLines.Push(currLine);
										currChars.Push(currChar);
										onceOrMoreB.Push(false);
										while (true) {
											indices.Push(index);
											currLines.Push(currLine);
											currChars.Push(currChar);
											if (index < length && source[index] == '*') {
												index++;
												currChar++;
											}
											else
												pass = false;
											if (pass) {
												onceOrMoreB.Pop();
												onceOrMoreB.Push(true);
												indices.Pop();
												currLines.Pop();
												currChars.Pop();
											}
											else {
												index = indices.Pop();
												currLine = currLines.Pop();
												currChar = currChars.Pop();
												break;
											}
										}
										pass = onceOrMoreB.Pop();
										if (pass) {
											indices.Pop();
											currLines.Pop();
											currChars.Pop();
										}
										else {
											index = indices.Pop();
											currLine = currLines.Pop();
											currChar = currChars.Pop();
										}
									}
								}
								if (pass) {
									indices.Pop();
									currLines.Pop();
									currChars.Pop();
								}
								else {
									pass = true;
									index = indices.Pop();
									currLine = currLines.Pop();
									currChar = currChars.Pop();
									break;
								}
							}
							if (pass) {
								if (index < length && source[index] == '/') {
									index++;
									currChar++;
								}
								else
									pass = false;
							}
						}
					}
				}
				if (pass) {
					indices.Pop();
					currLines.Pop();
					currChars.Pop();
					tokens.Add(new Token(null, NodeType.Tok_MultiLineComment, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar));
					continue;
				}
				else {
					index = indices.Pop();
					currLine = currLines.Pop();
					currChar = currChars.Pop();
				}

				//Token "Whitespace"
				pass = true;
				indices.Push(index);
				currLines.Push(currLine);
				currChars.Push(currChar);
				onceOrMoreB.Push(false);
				while (true) {
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					if (index < length && ((c = source[index]) == '\t' || c == ' ' || c == '\r' || c == '\n')) {
						index++;
						if (c == '\r') {
						}
						else
							if (c == '\n') {
								currLine++;
								currChar = 1;
							}
							else
								currChar++;
					}
					else
						pass = false;
					if (pass) {
						onceOrMoreB.Pop();
						onceOrMoreB.Push(true);
						indices.Pop();
						currLines.Pop();
						currChars.Pop();
					}
					else {
						index = indices.Pop();
						currLine = currLines.Pop();
						currChar = currChars.Pop();
						break;
					}
				}
				pass = onceOrMoreB.Pop();
				if (pass) {
					indices.Pop();
					currLines.Pop();
					currChars.Pop();
				}
				else {
					index = indices.Pop();
					currLine = currLines.Pop();
					currChar = currChars.Pop();
				}
				if (pass) {
					tokens.Add(new Token(null, NodeType.Tok_Whitespace, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar));
					continue;
				}

				break;
			}

			return tokens.ToArray();
		}

		private Token nextToken(bool useFetched) {
			if (useFetched && fetchedTokens.Count != 0) {
				Token tok = fetchedTokens[0];
				fetchedTokens.RemoveAt(0);
				return tok;
			}

			Token[] fillers = getFillerTokens();

			if (index == length)
				return new Token(fillers, NodeType.Tok_EOF, "", currLine, currChar);

			indices.Clear();
			currLines.Clear();
			currChars.Clear();
			char c;
			bool pass;
			int oldIndex = index, lastLine = currLine, lastChar = currChar;

			//Token "Assign"
			pass = true;
			if (index < length && source[index] == '=') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
				return new Token(fillers, NodeType.Tok_Assign, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);

			//Token "Comma"
			pass = true;
			if (index < length && source[index] == ',') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
				return new Token(fillers, NodeType.Tok_Comma, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);

			//Token "LBrace"
			pass = true;
			if (index < length && source[index] == '{') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
				return new Token(fillers, NodeType.Tok_LBrace, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);

			//Token "RBrace"
			pass = true;
			if (index < length && source[index] == '}') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
				return new Token(fillers, NodeType.Tok_RBrace, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);

			//Token "Name"
			pass = true;
			indices.Push(index);
			currLines.Push(currLine);
			currChars.Push(currChar);
			indices.Push(index);
			currLines.Push(currLine);
			currChars.Push(currChar);
			if (index < length && source[index] == '@') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass) {
				indices.Pop();
				currLines.Pop();
				currChars.Pop();
			}
			else {
				pass = true;
				index = indices.Pop();
				currLine = currLines.Pop();
				currChar = currChars.Pop();
			}
			if (pass) {
				if (index < length && (c = source[index]) >= 'A' && c <= 'Z') {
					index++;
					currChar++;
				}
				else
					pass = false;
				if (!pass) {
					pass = true;
					if (index < length && (c = source[index]) >= 'a' && c <= 'z') {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (!pass) {
						pass = true;
						if (index < length && source[index] == '_') {
							index++;
							currChar++;
						}
						else
							pass = false;
					}
				}
				if (pass) {
					while (true) {
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						if (index < length && (c = source[index]) >= 'A' && c <= 'Z') {
							index++;
							currChar++;
						}
						else
							pass = false;
						if (!pass) {
							pass = true;
							if (index < length && (c = source[index]) >= 'a' && c <= 'z') {
								index++;
								currChar++;
							}
							else
								pass = false;
							if (!pass) {
								pass = true;
								if (index < length && source[index] == '_') {
									index++;
									currChar++;
								}
								else
									pass = false;
							}
						}
						if (!pass) {
							pass = true;
							if (index < length && (c = source[index]) >= '0' && c <= '9') {
								index++;
								currChar++;
							}
							else
								pass = false;
						}
						if (pass) {
							indices.Pop();
							currLines.Pop();
							currChars.Pop();
						}
						else {
							pass = true;
							index = indices.Pop();
							currLine = currLines.Pop();
							currChar = currChars.Pop();
							break;
						}
					}
				}
			}
			if (pass) {
				indices.Pop();
				currLines.Pop();
				currChars.Pop();
				return new Token(fillers, NodeType.Tok_Name, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);
			}
			else {
				index = indices.Pop();
				currLine = currLines.Pop();
				currChar = currChars.Pop();
			}

			//Token "StringLiteral"
			pass = true;
			indices.Push(index);
			currLines.Push(currLine);
			currChars.Push(currChar);
			if (index < length && source[index] == '\"') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass) {
				while (true) {
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					if (index < length && source[index] == '\\') {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (pass) {
						if (index < length && ((c = source[index]) == 'r' || c == 'n' || c == 't' || c == '\"' || c == '\\')) {
							index++;
							currChar++;
						}
						else
							pass = false;
					}
					if (!pass) {
						pass = true;
						index = indices.Peek();
						currLine = currLines.Peek();
						currChar = currChars.Peek();
						if (index < length && (c = source[index]) != '\"' && c != '\\') {
							index++;
							if (c == '\r') {
							}
							else
								if (c == '\n') {
									currLine++;
									currChar = 1;
								}
								else
									currChar++;
						}
						else
							pass = false;
					}
					indices.Pop();
					currLines.Pop();
					currChars.Pop();
					if (pass) {
						indices.Pop();
						currLines.Pop();
						currChars.Pop();
					}
					else {
						pass = true;
						index = indices.Pop();
						currLine = currLines.Pop();
						currChar = currChars.Pop();
						break;
					}
				}
				if (pass) {
					if (index < length && source[index] == '\"') {
						index++;
						currChar++;
					}
					else
						pass = false;
				}
			}
			if (pass) {
				indices.Pop();
				currLines.Pop();
				currChars.Pop();
				return new Token(fillers, NodeType.Tok_StringLiteral, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);
			}
			else {
				index = indices.Pop();
				currLine = currLines.Pop();
				currChar = currChars.Pop();
			}

			//Token "FloatLiteral"
			pass = true;
			indices.Push(index);
			currLines.Push(currLine);
			currChars.Push(currChar);
			indices.Push(index);
			currLines.Push(currLine);
			currChars.Push(currChar);
			if (index < length && ((c = source[index]) == '+' || c == '-')) {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass) {
				indices.Pop();
				currLines.Pop();
				currChars.Pop();
			}
			else {
				pass = true;
				index = indices.Pop();
				currLine = currLines.Pop();
				currChar = currChars.Pop();
			}
			if (pass) {
				indices.Push(index);
				currLines.Push(currLine);
				currChars.Push(currChar);
				indices.Push(index);
				currLines.Push(currLine);
				currChars.Push(currChar);
				indices.Push(index);
				currLines.Push(currLine);
				currChars.Push(currChar);
				onceOrMoreB.Push(false);
				while (true) {
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					if (index < length && (c = source[index]) >= '0' && c <= '9') {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (pass) {
						onceOrMoreB.Pop();
						onceOrMoreB.Push(true);
						indices.Pop();
						currLines.Pop();
						currChars.Pop();
					}
					else {
						index = indices.Pop();
						currLine = currLines.Pop();
						currChar = currChars.Pop();
						break;
					}
				}
				pass = onceOrMoreB.Pop();
				if (pass) {
					indices.Pop();
					currLines.Pop();
					currChars.Pop();
				}
				else {
					index = indices.Pop();
					currLine = currLines.Pop();
					currChar = currChars.Pop();
				}
				if (pass) {
					if (index < length && source[index] == '.') {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (pass) {
						while (true) {
							indices.Push(index);
							currLines.Push(currLine);
							currChars.Push(currChar);
							if (index < length && (c = source[index]) >= '0' && c <= '9') {
								index++;
								currChar++;
							}
							else
								pass = false;
							if (pass) {
								indices.Pop();
								currLines.Pop();
								currChars.Pop();
							}
							else {
								pass = true;
								index = indices.Pop();
								currLine = currLines.Pop();
								currChar = currChars.Pop();
								break;
							}
						}
					}
				}
				if (!pass) {
					pass = true;
					index = indices.Peek();
					currLine = currLines.Peek();
					currChar = currChars.Peek();
					if (index < length && source[index] == '.') {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (pass) {
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						onceOrMoreB.Push(false);
						while (true) {
							indices.Push(index);
							currLines.Push(currLine);
							currChars.Push(currChar);
							if (index < length && (c = source[index]) >= '0' && c <= '9') {
								index++;
								currChar++;
							}
							else
								pass = false;
							if (pass) {
								onceOrMoreB.Pop();
								onceOrMoreB.Push(true);
								indices.Pop();
								currLines.Pop();
								currChars.Pop();
							}
							else {
								index = indices.Pop();
								currLine = currLines.Pop();
								currChar = currChars.Pop();
								break;
							}
						}
						pass = onceOrMoreB.Pop();
						if (pass) {
							indices.Pop();
							currLines.Pop();
							currChars.Pop();
						}
						else {
							index = indices.Pop();
							currLine = currLines.Pop();
							currChar = currChars.Pop();
						}
					}
				}
				indices.Pop();
				currLines.Pop();
				currChars.Pop();
				if (pass) {
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					if (index < length && ((c = source[index]) == 'E' || c == 'e')) {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (pass) {
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						if (index < length && ((c = source[index]) == '+' || c == '-')) {
							index++;
							currChar++;
						}
						else
							pass = false;
						if (pass) {
							indices.Pop();
							currLines.Pop();
							currChars.Pop();
						}
						else {
							pass = true;
							index = indices.Pop();
							currLine = currLines.Pop();
							currChar = currChars.Pop();
						}
						if (pass) {
							indices.Push(index);
							currLines.Push(currLine);
							currChars.Push(currChar);
							onceOrMoreB.Push(false);
							while (true) {
								indices.Push(index);
								currLines.Push(currLine);
								currChars.Push(currChar);
								if (index < length && (c = source[index]) >= '0' && c <= '9') {
									index++;
									currChar++;
								}
								else
									pass = false;
								if (pass) {
									onceOrMoreB.Pop();
									onceOrMoreB.Push(true);
									indices.Pop();
									currLines.Pop();
									currChars.Pop();
								}
								else {
									index = indices.Pop();
									currLine = currLines.Pop();
									currChar = currChars.Pop();
									break;
								}
							}
							pass = onceOrMoreB.Pop();
							if (pass) {
								indices.Pop();
								currLines.Pop();
								currChars.Pop();
							}
							else {
								index = indices.Pop();
								currLine = currLines.Pop();
								currChar = currChars.Pop();
							}
						}
					}
					if (pass) {
						indices.Pop();
						currLines.Pop();
						currChars.Pop();
					}
					else {
						pass = true;
						index = indices.Pop();
						currLine = currLines.Pop();
						currChar = currChars.Pop();
					}
				}
				if (!pass) {
					pass = true;
					index = indices.Peek();
					currLine = currLines.Peek();
					currChar = currChars.Peek();
					if (index < length && source[index] == '0') {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (!pass) {
						pass = true;
						if (index < length && (c = source[index]) >= '1' && c <= '9') {
							index++;
							currChar++;
						}
						else
							pass = false;
						if (pass) {
							while (true) {
								indices.Push(index);
								currLines.Push(currLine);
								currChars.Push(currChar);
								if (index < length && (c = source[index]) >= '0' && c <= '9') {
									index++;
									currChar++;
								}
								else
									pass = false;
								if (pass) {
									indices.Pop();
									currLines.Pop();
									currChars.Pop();
								}
								else {
									pass = true;
									index = indices.Pop();
									currLine = currLines.Pop();
									currChar = currChars.Pop();
									break;
								}
							}
						}
					}
					if (pass) {
						if (index < length && ((c = source[index]) == 'E' || c == 'e')) {
							index++;
							currChar++;
						}
						else
							pass = false;
						if (pass) {
							indices.Push(index);
							currLines.Push(currLine);
							currChars.Push(currChar);
							if (index < length && ((c = source[index]) == '+' || c == '-')) {
								index++;
								currChar++;
							}
							else
								pass = false;
							if (pass) {
								indices.Pop();
								currLines.Pop();
								currChars.Pop();
							}
							else {
								pass = true;
								index = indices.Pop();
								currLine = currLines.Pop();
								currChar = currChars.Pop();
							}
							if (pass) {
								indices.Push(index);
								currLines.Push(currLine);
								currChars.Push(currChar);
								onceOrMoreB.Push(false);
								while (true) {
									indices.Push(index);
									currLines.Push(currLine);
									currChars.Push(currChar);
									if (index < length && (c = source[index]) >= '0' && c <= '9') {
										index++;
										currChar++;
									}
									else
										pass = false;
									if (pass) {
										onceOrMoreB.Pop();
										onceOrMoreB.Push(true);
										indices.Pop();
										currLines.Pop();
										currChars.Pop();
									}
									else {
										index = indices.Pop();
										currLine = currLines.Pop();
										currChar = currChars.Pop();
										break;
									}
								}
								pass = onceOrMoreB.Pop();
								if (pass) {
									indices.Pop();
									currLines.Pop();
									currChars.Pop();
								}
								else {
									index = indices.Pop();
									currLine = currLines.Pop();
									currChar = currChars.Pop();
								}
							}
						}
					}
				}
				indices.Pop();
				currLines.Pop();
				currChars.Pop();
			}
			if (pass) {
				indices.Pop();
				currLines.Pop();
				currChars.Pop();
				return new Token(fillers, NodeType.Tok_FloatLiteral, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);
			}
			else {
				index = indices.Pop();
				currLine = currLines.Pop();
				currChar = currChars.Pop();
			}

			//Token "IntLiteral"
			pass = true;
			indices.Push(index);
			currLines.Push(currLine);
			currChars.Push(currChar);
			if (index < length && source[index] == '0') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass) {
				if (index < length && ((c = source[index]) == 'X' || c == 'x')) {
					index++;
					currChar++;
				}
				else
					pass = false;
				if (pass) {
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					onceOrMoreB.Push(false);
					while (true) {
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						if (index < length && (c = source[index]) >= '0' && c <= '9') {
							index++;
							currChar++;
						}
						else
							pass = false;
						if (!pass) {
							pass = true;
							if (index < length && (c = source[index]) >= 'A' && c <= 'F') {
								index++;
								currChar++;
							}
							else
								pass = false;
							if (!pass) {
								pass = true;
								if (index < length && (c = source[index]) >= 'a' && c <= 'f') {
									index++;
									currChar++;
								}
								else
									pass = false;
							}
						}
						if (pass) {
							onceOrMoreB.Pop();
							onceOrMoreB.Push(true);
							indices.Pop();
							currLines.Pop();
							currChars.Pop();
						}
						else {
							index = indices.Pop();
							currLine = currLines.Pop();
							currChar = currChars.Pop();
							break;
						}
					}
					pass = onceOrMoreB.Pop();
					if (pass) {
						indices.Pop();
						currLines.Pop();
						currChars.Pop();
					}
					else {
						index = indices.Pop();
						currLine = currLines.Pop();
						currChar = currChars.Pop();
					}
				}
			}
			if (!pass) {
				pass = true;
				index = indices.Peek();
				currLine = currLines.Peek();
				currChar = currChars.Peek();
				indices.Push(index);
				currLines.Push(currLine);
				currChars.Push(currChar);
				if (index < length && ((c = source[index]) == '+' || c == '-')) {
					index++;
					currChar++;
				}
				else
					pass = false;
				if (pass) {
					indices.Pop();
					currLines.Pop();
					currChars.Pop();
				}
				else {
					pass = true;
					index = indices.Pop();
					currLine = currLines.Pop();
					currChar = currChars.Pop();
				}
				if (pass) {
					if (index < length && source[index] == '0') {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (!pass) {
						pass = true;
						if (index < length && (c = source[index]) >= '1' && c <= '9') {
							index++;
							currChar++;
						}
						else
							pass = false;
						if (pass) {
							while (true) {
								indices.Push(index);
								currLines.Push(currLine);
								currChars.Push(currChar);
								if (index < length && (c = source[index]) >= '0' && c <= '9') {
									index++;
									currChar++;
								}
								else
									pass = false;
								if (pass) {
									indices.Pop();
									currLines.Pop();
									currChars.Pop();
								}
								else {
									pass = true;
									index = indices.Pop();
									currLine = currLines.Pop();
									currChar = currChars.Pop();
									break;
								}
							}
						}
					}
				}
			}
			indices.Pop();
			currLines.Pop();
			currChars.Pop();
			if (pass)
				return new Token(fillers, NodeType.Tok_IntLiteral, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);

			throw new ParserException("Line " + currLine + ", char " + currChar + ": No token match");
		}

		private RuleInstance matchStart() {
			List<Node> nodes = new List<Node>();
			Token tok;

			while (true) {
				if ((tok = fetchToken(laOffset)).Type == NodeType.Tok_KeyFalse || tok.Type == NodeType.Tok_KeyTrue || tok.Type == NodeType.Tok_Name) {
					laOffsets.Push(laOffset);
					laSuccess.Push(true);
					lookaheadAnyName();
					if (laSuccess.Peek()) {
						if (fetchToken(laOffset).Type == NodeType.Tok_LBrace)
							laOffset++;
						else {
							laSuccess.Pop();
							laSuccess.Push(false);
						}
					}
					laOffset = laOffsets.Pop();
					if (laSuccess.Pop()) {
						nodes.Add(matchBlock());
					}
					else {
						nodes.Add(matchProperty());
					}
				}
				else
					break;
			}
			if ((tok = nextToken()).Type != NodeType.Tok_EOF)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected EOF token");
			nodes.Add(tok);

			return new RuleInstance(NodeType.Rule_Start, nodes.ToArray());
		}

		private RuleInstance matchProperty() {
			List<Node> nodes = new List<Node>();

			laOffsets.Push(laOffset);
			laSuccess.Push(true);
			lookaheadBoolProperty();
			laOffset = laOffsets.Pop();
			if (laSuccess.Pop()) {
				nodes.Add(matchBoolProperty());
			}
			else {
				laOffsets.Push(laOffset);
				laSuccess.Push(true);
				lookaheadEnumProperty();
				laOffset = laOffsets.Pop();
				if (laSuccess.Pop()) {
					nodes.Add(matchEnumProperty());
				}
				else {
					laOffsets.Push(laOffset);
					laSuccess.Push(true);
					lookaheadAnyName();
					if (laSuccess.Peek()) {
						if (fetchToken(laOffset).Type == NodeType.Tok_Assign)
							laOffset++;
						else {
							laSuccess.Pop();
							laSuccess.Push(false);
						}
					}
					if (laSuccess.Peek()) {
						if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral) {
							if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral)
								laOffset++;
							else {
								laSuccess.Pop();
								laSuccess.Push(false);
							}
						}
						else {
							if (fetchToken(laOffset).Type == NodeType.Tok_FloatLiteral)
								laOffset++;
							else {
								laSuccess.Pop();
								laSuccess.Push(false);
							}
						}
					}
					if (laSuccess.Peek()) {
						if (fetchToken(laOffset).Type == NodeType.Tok_Comma)
							laOffset++;
						else {
							laSuccess.Pop();
							laSuccess.Push(false);
						}
					}
					if (laSuccess.Peek()) {
						if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral) {
							if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral)
								laOffset++;
							else {
								laSuccess.Pop();
								laSuccess.Push(false);
							}
						}
						else {
							if (fetchToken(laOffset).Type == NodeType.Tok_FloatLiteral)
								laOffset++;
							else {
								laSuccess.Pop();
								laSuccess.Push(false);
							}
						}
					}
					if (laSuccess.Peek()) {
						if (fetchToken(laOffset).Type == NodeType.Tok_Comma)
							laOffset++;
						else {
							laSuccess.Pop();
							laSuccess.Push(false);
						}
					}
					if (laSuccess.Peek()) {
						if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral) {
							if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral)
								laOffset++;
							else {
								laSuccess.Pop();
								laSuccess.Push(false);
							}
						}
						else {
							if (fetchToken(laOffset).Type == NodeType.Tok_FloatLiteral)
								laOffset++;
							else {
								laSuccess.Pop();
								laSuccess.Push(false);
							}
						}
					}
					if (laSuccess.Peek()) {
						if (fetchToken(laOffset).Type == NodeType.Tok_Comma)
							laOffset++;
						else {
							laSuccess.Pop();
							laSuccess.Push(false);
						}
					}
					laOffset = laOffsets.Pop();
					if (laSuccess.Pop()) {
						nodes.Add(matchQuatProperty());
					}
					else {
						laOffsets.Push(laOffset);
						laSuccess.Push(true);
						lookaheadAnyName();
						if (laSuccess.Peek()) {
							if (fetchToken(laOffset).Type == NodeType.Tok_Assign)
								laOffset++;
							else {
								laSuccess.Pop();
								laSuccess.Push(false);
							}
						}
						if (laSuccess.Peek()) {
							if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral) {
								if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral)
									laOffset++;
								else {
									laSuccess.Pop();
									laSuccess.Push(false);
								}
							}
							else {
								if (fetchToken(laOffset).Type == NodeType.Tok_FloatLiteral)
									laOffset++;
								else {
									laSuccess.Pop();
									laSuccess.Push(false);
								}
							}
						}
						if (laSuccess.Peek()) {
							if (fetchToken(laOffset).Type == NodeType.Tok_Comma)
								laOffset++;
							else {
								laSuccess.Pop();
								laSuccess.Push(false);
							}
						}
						laOffset = laOffsets.Pop();
						if (laSuccess.Pop()) {
							nodes.Add(matchVec3Property());
						}
						else {
							laOffsets.Push(laOffset);
							laSuccess.Push(true);
							lookaheadNumericProperty();
							laOffset = laOffsets.Pop();
							if (laSuccess.Pop()) {
								nodes.Add(matchNumericProperty());
							}
							else {
								nodes.Add(matchStringProperty());
							}
						}
					}
				}
			}

			return new RuleInstance(NodeType.Rule_Property, nodes.ToArray());
		}

		private RuleInstance matchEnumProperty() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_Assign)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected Assign token");
			nodes.Add(tok);
			nodes.Add(matchAnyName());

			return new RuleInstance(NodeType.Rule_EnumProperty, nodes.ToArray());
		}

		private RuleInstance matchQuatProperty() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_Assign)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected Assign token");
			nodes.Add(tok);
			if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral) {
				nodes.Add(nextToken());
			}
			else {
				if ((tok = nextToken()).Type != NodeType.Tok_FloatLiteral)
					throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected FloatLiteral token");
				nodes.Add(tok);
			}
			if ((tok = nextToken()).Type != NodeType.Tok_Comma)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected Comma token");
			nodes.Add(tok);
			if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral) {
				nodes.Add(nextToken());
			}
			else {
				if ((tok = nextToken()).Type != NodeType.Tok_FloatLiteral)
					throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected FloatLiteral token");
				nodes.Add(tok);
			}
			if ((tok = nextToken()).Type != NodeType.Tok_Comma)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected Comma token");
			nodes.Add(tok);
			if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral) {
				nodes.Add(nextToken());
			}
			else {
				if ((tok = nextToken()).Type != NodeType.Tok_FloatLiteral)
					throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected FloatLiteral token");
				nodes.Add(tok);
			}
			if ((tok = nextToken()).Type != NodeType.Tok_Comma)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected Comma token");
			nodes.Add(tok);
			if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral) {
				nodes.Add(nextToken());
			}
			else {
				if ((tok = nextToken()).Type != NodeType.Tok_FloatLiteral)
					throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected FloatLiteral token");
				nodes.Add(tok);
			}

			return new RuleInstance(NodeType.Rule_QuatProperty, nodes.ToArray());
		}

		private RuleInstance matchVec3Property() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_Assign)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected Assign token");
			nodes.Add(tok);
			if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral) {
				nodes.Add(nextToken());
			}
			else {
				if ((tok = nextToken()).Type != NodeType.Tok_FloatLiteral)
					throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected FloatLiteral token");
				nodes.Add(tok);
			}
			if ((tok = nextToken()).Type != NodeType.Tok_Comma)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected Comma token");
			nodes.Add(tok);
			if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral) {
				nodes.Add(nextToken());
			}
			else {
				if ((tok = nextToken()).Type != NodeType.Tok_FloatLiteral)
					throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected FloatLiteral token");
				nodes.Add(tok);
			}
			if ((tok = nextToken()).Type != NodeType.Tok_Comma)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected Comma token");
			nodes.Add(tok);
			if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral) {
				nodes.Add(nextToken());
			}
			else {
				if ((tok = nextToken()).Type != NodeType.Tok_FloatLiteral)
					throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected FloatLiteral token");
				nodes.Add(tok);
			}

			return new RuleInstance(NodeType.Rule_Vec3Property, nodes.ToArray());
		}

		private RuleInstance matchNumericProperty() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_Assign)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected Assign token");
			nodes.Add(tok);
			if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral) {
				nodes.Add(nextToken());
			}
			else {
				if ((tok = nextToken()).Type != NodeType.Tok_FloatLiteral)
					throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected FloatLiteral token");
				nodes.Add(tok);
			}

			return new RuleInstance(NodeType.Rule_NumericProperty, nodes.ToArray());
		}

		private RuleInstance matchStringProperty() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_Assign)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected Assign token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_StringLiteral)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected StringLiteral token");
			nodes.Add(tok);

			return new RuleInstance(NodeType.Rule_StringProperty, nodes.ToArray());
		}

		private RuleInstance matchBoolProperty() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_Assign)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected Assign token");
			nodes.Add(tok);
			if (fetchToken(laOffset).Type == NodeType.Tok_KeyTrue) {
				nodes.Add(nextToken());
			}
			else {
				if ((tok = nextToken()).Type != NodeType.Tok_KeyFalse)
					throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected KeyFalse token");
				nodes.Add(tok);
			}

			return new RuleInstance(NodeType.Rule_BoolProperty, nodes.ToArray());
		}

		private RuleInstance matchBlock() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_LBrace)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected LBrace token");
			nodes.Add(tok);
			while (true) {
				if ((tok = fetchToken(laOffset)).Type == NodeType.Tok_KeyFalse || tok.Type == NodeType.Tok_KeyTrue || tok.Type == NodeType.Tok_Name) {
					nodes.Add(matchProperty());
				}
				else
					break;
			}
			if ((tok = nextToken()).Type != NodeType.Tok_RBrace)
				throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected RBrace token");
			nodes.Add(tok);

			return new RuleInstance(NodeType.Rule_Block, nodes.ToArray());
		}

		private RuleInstance matchAnyName() {
			List<Node> nodes = new List<Node>();
			Token tok;

			if (fetchToken(laOffset).Type == NodeType.Tok_KeyFalse) {
				nodes.Add(nextToken());
			}
			else {
				if (fetchToken(laOffset).Type == NodeType.Tok_KeyTrue) {
					nodes.Add(nextToken());
				}
				else {
					if ((tok = nextToken()).Type != NodeType.Tok_Name)
						throw new ParserException("Line " + tok.LineNr + ", char " + tok.CharNr + ": Expected Name token");
					nodes.Add(tok);
				}
			}

			return new RuleInstance(NodeType.Rule_AnyName, nodes.ToArray());
		}

		private void lookaheadEnumProperty() {
			lookaheadAnyName();
			if (laSuccess.Peek()) {
				if (fetchToken(laOffset).Type == NodeType.Tok_Assign)
					laOffset++;
				else {
					laSuccess.Pop();
					laSuccess.Push(false);
				}
			}
			if (laSuccess.Peek()) {
				lookaheadAnyName();
			}
		}

		private void lookaheadNumericProperty() {
			lookaheadAnyName();
			if (laSuccess.Peek()) {
				if (fetchToken(laOffset).Type == NodeType.Tok_Assign)
					laOffset++;
				else {
					laSuccess.Pop();
					laSuccess.Push(false);
				}
			}
			if (laSuccess.Peek()) {
				if (fetchToken(laOffset).Type == NodeType.Tok_IntLiteral) {
					laOffset++;
				}
				else {
					if (fetchToken(laOffset).Type == NodeType.Tok_FloatLiteral)
						laOffset++;
					else {
						laSuccess.Pop();
						laSuccess.Push(false);
					}
				}
			}
		}

		private void lookaheadBoolProperty() {
			lookaheadAnyName();
			if (laSuccess.Peek()) {
				if (fetchToken(laOffset).Type == NodeType.Tok_Assign)
					laOffset++;
				else {
					laSuccess.Pop();
					laSuccess.Push(false);
				}
			}
			if (laSuccess.Peek()) {
				if (fetchToken(laOffset).Type == NodeType.Tok_KeyTrue) {
					laOffset++;
				}
				else {
					if (fetchToken(laOffset).Type == NodeType.Tok_KeyFalse)
						laOffset++;
					else {
						laSuccess.Pop();
						laSuccess.Push(false);
					}
				}
			}
		}

		private void lookaheadAnyName() {
			if (fetchToken(laOffset).Type == NodeType.Tok_KeyFalse) {
				laOffset++;
			}
			else {
				if (fetchToken(laOffset).Type == NodeType.Tok_KeyTrue) {
					laOffset++;
				}
				else {
					if (fetchToken(laOffset).Type == NodeType.Tok_Name)
						laOffset++;
					else {
						laSuccess.Pop();
						laSuccess.Push(false);
					}
				}
			}
		}
	}
}
