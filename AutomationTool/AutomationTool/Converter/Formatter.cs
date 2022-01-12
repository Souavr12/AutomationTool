/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

namespace AutomationTool.Converter
{
    internal static class Formatter
    {
        private static string _indent = "   ";

        /// <summary>
        /// Appends the indent
        /// </summary>
        /// <param name="sb">StringBuilder</param>
        /// <param name="count">integer</param>
        public static void AppendIndent(System.Text.StringBuilder sb, int count)
        {
            try
            {
                for (; count > 0; --count) sb.Append(_indent);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message + System.Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + System.Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// beautifies the values with specified values
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>returned beautified string</returns>
        public static string PrettyPrint(string input)
        {
            try
            {
                return PrettyPrint(input, "   ");
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message + System.Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + System.Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// beautifies the values with specified spaces
        /// </summary>
        /// <param name="input">input string</param>
        /// <param name="spaces">number of spaces</param>
        /// <returns>returned beautified string</returns>
        public static string PrettyPrint(string input, string spaces)
        {
            try
            {
                _indent = spaces;
                var output = new System.Text.StringBuilder();
                int depth = 0;
                int len = input.Length;
                char[] chars = input.ToCharArray();
                for (int i = 0; i < len; ++i)
                {
                    char ch = chars[i];

                    if (ch == '\"') // found string span
                    {
                        bool str = true;
                        while (str)
                        {
                            output.Append(ch);
                            ch = chars[++i];
                            if (ch == '\\')
                            {
                                output.Append(ch);
                                ch = chars[++i];
                            }
                            else if (ch == '\"')
                                str = false;
                        }
                    }

                    switch (ch)
                    {
                        case '{':
                        case '[':
                            output.Append(ch);
                            output.AppendLine();
                            AppendIndent(output, ++depth);
                            break;
                        case '}':
                        case ']':
                            output.AppendLine();
                            AppendIndent(output, --depth);
                            output.Append(ch);
                            break;
                        case ',':
                            output.Append(ch);
                            output.AppendLine();
                            AppendIndent(output, depth);
                            break;
                        case ':':
                            output.Append(" : ");
                            break;
                        default:
                            if (!char.IsWhiteSpace(ch))
                                output.Append(ch);
                            break;
                    }
                }

                return output.ToString();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message + System.Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + System.Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}