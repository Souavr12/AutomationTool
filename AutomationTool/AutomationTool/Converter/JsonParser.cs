﻿/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AutomationTool.Converter
{
    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// </summary>
    internal sealed class JsonParser
    {
        #region Properties and Variables
        enum Token
        {
            None = -1,           // Used to denote no Lookahead available
            Curly_Open,
            Curly_Close,
            Squared_Open,
            Squared_Close,
            Colon,
            Comma,
            String,
            Number,
            True,
            False,
            Null
        }

        readonly string json;
        readonly StringBuilder s = new StringBuilder(); // used for inner string parsing " \"\r\n\u1234\'\t " 
        Token lookAheadToken = Token.None;
        int index;
        bool allownonquotedkey = false;
        #endregion

        #region Constructors
        /// <summary>
        /// parameterized Constructor taking JSON value and confirmation for allowing non-quoted keys
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <param name="AllowNonQuotedKeys">'True' if alowed for non-quoted keys.</param>
        internal JsonParser(string json, bool AllowNonQuotedKeys)
        {
            this.allownonquotedkey = AllowNonQuotedKeys;
            this.json = json;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Decode the object
        /// </summary>
        /// <returns>the decoded object</returns>
        public object Decode()
        {
            try
            {
                return ParseValue(false);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Parses the object
        /// </summary>
        /// <returns>Dictionary in string and object format</returns>
        private Dictionary<string, object> ParseObject()
        {
            try
            {
                Dictionary<string, object> table = new Dictionary<string, object>();

                ConsumeToken(); // {

                while (true)
                {
                    switch (LookAhead())
                    {

                        case Token.Comma:
                            ConsumeToken();
                            break;

                        case Token.Curly_Close:
                            ConsumeToken();
                            return table;

                        default:
                            {
                                // name
                                string name = ParseString(false);

                                var n = NextToken();
                                // :
                                if (n != Token.Colon)
                                {
                                    throw new Exception("Expected colon at index " + index);
                                }

                                // value
                                object value = ParseValue(true);

                                table[name] = value;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Parses Array
        /// </summary>
        /// <returns>List of object</returns>
        private List<object> ParseArray()
        {
            try
            {
                List<object> array = new List<object>();
                ConsumeToken(); // [

                while (true)
                {
                    switch (LookAhead())
                    {
                        case Token.Comma:
                            ConsumeToken();
                            break;

                        case Token.Squared_Close:
                            ConsumeToken();
                            return array;

                        default:
                            array.Add(ParseValue(false));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Parses Values
        /// </summary>
        /// <param name="val">value in boolean format</param>
        /// <returns>The parsed object</returns>
        private object ParseValue(bool val)
        {
            try
            {
                switch (LookAhead())
                {
                    case Token.Number:
                        return ParseNumber();

                    case Token.String:
                        return ParseString(val);

                    case Token.Curly_Open:
                        return ParseObject();

                    case Token.Squared_Open:
                        return ParseArray();

                    case Token.True:
                        ConsumeToken();
                        return true;

                    case Token.False:
                        ConsumeToken();
                        return false;

                    case Token.Null:
                        ConsumeToken();
                        return null;
                }

                throw new Exception("Unrecognized token at index" + index);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Parses Strings
        /// </summary>
        /// <param name="val">value in boolean format</param>
        /// <returns>The parsed object</returns>
        private string ParseString(bool val)
        {
            try
            {
                ConsumeToken(); // "

                s.Length = 0;
                bool instr = val;
                int runIndex = -1;
                int l = json.Length;
                //fixed (char* p = json)
                string p = json;
                {
                    while (index < l)
                    {
                        var c = p[index++];
                        if (c == '"')
                            instr = true;

                        if (c == '"' || (allownonquotedkey && (c == ':' || c == ' ' || c == '\t') && instr == false))
                        {
                            int len = 1;
                            if (allownonquotedkey && c != '"' && instr == false)
                            {
                                index--;
                                //index--;
                                len = 0;
                                //runIndex--;
                            }

                            if (runIndex != -1)
                            {
                                if (s.Length == 0)
                                    return json.Substring(runIndex, index - runIndex - len);

                                s.Append(json, runIndex, index - runIndex - 1);
                            }
                            return s.ToString();
                        }

                        if (c != '\\')
                        {
                            if (runIndex == -1)
                                runIndex = index - 1;

                            continue;
                        }

                        if (index == l) break;

                        if (runIndex != -1)
                        {
                            s.Append(json, runIndex, index - runIndex - 1);
                            runIndex = -1;
                        }

                        switch (p[index++])
                        {
                            case '"':
                                s.Append('"');
                                break;

                            case '\\':
                                s.Append('\\');
                                break;

                            case '/':
                                s.Append('/');
                                break;

                            case 'b':
                                s.Append('\b');
                                break;

                            case 'f':
                                s.Append('\f');
                                break;

                            case 'n':
                                s.Append('\n');
                                break;

                            case 'r':
                                s.Append('\r');
                                break;

                            case 't':
                                s.Append('\t');
                                break;

                            case 'u':
                                {
                                    int remainingLength = l - index;
                                    if (remainingLength < 4) break;

                                    // parse the 32 bit hex into an integer codepoint
                                    uint codePoint = ParseUnicode(p[index], p[index + 1], p[index + 2], p[index + 3]);
                                    s.Append((char)codePoint);

                                    // skip 4 chars
                                    index += 4;
                                }
                                break;
                        }
                    }
                }

                throw new Exception("Unexpectedly reached end of string");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Parses Strings characters
        /// </summary>
        /// <param name="c1">Characters</param>
        /// <param name="multipliyer">MultiPliyer</param>
        /// <returns></returns>
        private uint ParseSingleChar(char c1, uint multipliyer)
        {
            try
            {
                uint p1 = 0;
                if (c1 >= '0' && c1 <= '9')
                    p1 = (uint)(c1 - '0') * multipliyer;
                else if (c1 >= 'A' && c1 <= 'F')
                    p1 = (uint)((c1 - 'A') + 10) * multipliyer;
                else if (c1 >= 'a' && c1 <= 'f')
                    p1 = (uint)((c1 - 'a') + 10) * multipliyer;
                return p1;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Parses Unicodes
        /// </summary>
        /// <param name="c1">Charater 1</param>
        /// <param name="c2">Charater 2</param>
        /// <param name="c3">Charater 3</param>
        /// <param name="c4">Charater 4</param>
        /// <returns>UniCode</returns>
        private uint ParseUnicode(char c1, char c2, char c3, char c4)
        {
            try
            {
                uint p1 = ParseSingleChar(c1, 0x1000);
                uint p2 = ParseSingleChar(c2, 0x100);
                uint p3 = ParseSingleChar(c3, 0x10);
                uint p4 = ParseSingleChar(c4, 1);

                return p1 + p2 + p3 + p4;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates Long values
        /// </summary>
        /// <param name="s">string values</param>
        /// <returns>Value in long</returns>
        private long CreateLong(string s)
        {
            try
            {
                long num = 0;
                bool neg = false;
                foreach (char cc in s)
                {
                    if (cc == '-')
                        neg = true;
                    else if (cc == '+')
                        neg = false;
                    else
                    {
                        num *= 10;
                        num += (int)(cc - '0');
                    }
                }

                return neg ? -num : num;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Parse number
        /// </summary>
        /// <returns>object</returns>
        private object ParseNumber()
        {
            try
            {
                ConsumeToken();

                // Need to start back one place because the first digit is also a token and would have been consumed
                var startIndex = index - 1;
                bool dec = false;
                do
                {
                    if (index == json.Length)
                        break;
                    var c = json[index];

                    if ((c >= '0' && c <= '9') || c == '.' || c == '-' || c == '+' || c == 'e' || c == 'E')
                    {
                        if (c == '.' || c == 'e' || c == 'E')
                            dec = true;
                        if (++index == json.Length)
                            break;//throw new Exception("Unexpected end of string whilst parsing number");
                        continue;
                    }
                    break;
                } while (true);

                if (dec)
                {
                    string s = json.Substring(startIndex, index - startIndex);
                    return double.Parse(s, NumberFormatInfo.InvariantInfo);
                }
                if (index - startIndex < 20 && json[startIndex] != '-')
                    return JSON.CreateLong(json, startIndex, index - startIndex);
                else
                {
                    string s = json.Substring(startIndex, index - startIndex);
                    //return s;
                    return decimal.Parse(s, NumberFormatInfo.InvariantInfo);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Looks ahead for the token
        /// </summary>
        /// <returns>The new token</returns>
        private Token LookAhead()
        {
            try
            {
                if (lookAheadToken != Token.None) return lookAheadToken;

                return lookAheadToken = NextTokenCore();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Consumes the token
        /// </summary>
        private void ConsumeToken()
        {
            try
            {
                lookAheadToken = Token.None;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Checks for the next token and if found then return
        /// </summary>
        /// <returns>Next token</returns>
        private Token NextToken()
        {
            try
            {
                var result = lookAheadToken != Token.None ? lookAheadToken : NextTokenCore();

                lookAheadToken = Token.None;

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Gets the next token core
        /// </summary>
        /// <returns>The token core</returns>
        private Token NextTokenCore()
        {
            try
            {
                char c;

                // Skip past whitespace
                do
                {
                    c = json[index];

                    if (c == '/' && json[index + 1] == '/') // c++ style single line comments
                    {
                        index++;
                        index++;
                        do
                        {
                            c = json[index];
                            if (c == '\r' || c == '\n') break; // read till end of line
                        }
                        while (++index < json.Length);
                    }
                    if (c > ' ') break;
                    if (c != ' ' && c != '\t' && c != '\n' && c != '\r') break;

                } while (++index < json.Length);

                if (index == json.Length)
                {
                    throw new Exception("Reached end of string unexpectedly");
                }

                c = json[index];

                index++;

                switch (c)
                {
                    case '{':
                        return Token.Curly_Open;

                    case '}':
                        return Token.Curly_Close;

                    case '[':
                        return Token.Squared_Open;

                    case ']':
                        return Token.Squared_Close;

                    case ',':
                        return Token.Comma;

                    case '"':
                        return Token.String;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                    case '+':
                    case '.':
                        return Token.Number;

                    case ':':
                        return Token.Colon;

                    case 'f':
                        if (json.Length - index >= 4 &&
                            json[index + 0] == 'a' &&
                            json[index + 1] == 'l' &&
                            json[index + 2] == 's' &&
                            json[index + 3] == 'e')
                        {
                            index += 4;
                            return Token.False;
                        }
                        break;

                    case 't':
                        if (json.Length - index >= 3 &&
                            json[index + 0] == 'r' &&
                            json[index + 1] == 'u' &&
                            json[index + 2] == 'e')
                        {
                            index += 3;
                            return Token.True;
                        }
                        break;

                    case 'n':
                        if (json.Length - index >= 3 &&
                            json[index + 0] == 'u' &&
                            json[index + 1] == 'l' &&
                            json[index + 2] == 'l')
                        {
                            index += 3;
                            return Token.Null;
                        }
                        break;
                }
                if (allownonquotedkey)
                {
                    index--;
                    return Token.String;
                }
                else
                    throw new Exception("Could not find token at index " + --index);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
    }
}