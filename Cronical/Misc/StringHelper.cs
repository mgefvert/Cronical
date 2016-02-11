using System;

namespace Cronical.Misc
{
  /// <summary>
  /// Helper extension class for dealing with strings.
  /// </summary>
  public static class StringHelper
  {
    /// <summary>
    /// Extract a portion of a string. If bounds fall outside of the existing string, they will be adjusted accordingly.
    /// </summary>
    /// <param name="str">String to extract from</param>
    /// <param name="startIndex">Zero-based start index</param>
    /// <param name="length">Number of characters to extract</param>
    /// <returns>The new string</returns>
    public static string Get(this string str, int startIndex, int length)
    {
      return SafeSubstring(str, startIndex, length);
    }

    /// <summary>
    /// Extract a portion of a string. If bounds fall outside of the existing string, they will be adjusted accordingly.
    /// The resulting string will be Trim()-ed before returning.
    /// </summary>
    /// <param name="str">String to extract from</param>
    /// <param name="startIndex">Zero-based start index</param>
    /// <param name="length">Number of characters to extract</param>
    /// <returns>The new string with Trim()-ed bounds</returns>
    public static string GetTrim(this string str, int startIndex, int length)
    {
      return SafeSubstring(str, startIndex, length).Trim();
    }

    /// <summary>
    /// Extract an integer from a portion of a string. If bounds fall outside of the existing string, 
    /// they will be adjusted accordingly.
    /// </summary>
    /// <param name="str">String to extract from</param>
    /// <param name="startIndex">Zero-based start index</param>
    /// <param name="length">Number of characters to extract</param>
    /// <param name="defaultValue">Default value to use (normally 0)</param>
    /// <returns>The extracted integer, or defaultValue if int.TryParse fails</returns>
    public static int GetInt(this string str, int startIndex, int length, int defaultValue = 0)
    {
      int result;
      return int.TryParse(SafeSubstring(str, startIndex, length), out result) ? result : defaultValue;
    }

    /// <summary>
    /// Return the last character of the string, or \0 if the string is empty
    /// </summary>
    /// <param name="str">The string to operate on</param>
    /// <returns>The last character of the string, or \0 if none found</returns>
    public static char LastChar(this string str)
    {
      return string.IsNullOrEmpty(str) ? '\0' : str[str.Length - 1];
    }

    /// <summary>
    /// Return a number of leftmost characters from a string
    /// </summary>
    /// <param name="str">String to operate on</param>
    /// <param name="length">Number of characters to extract</param>
    /// <returns>The new string</returns>
    public static string Left(this string str, int length)
    {
      return SafeSubstring(str, 0, length);
    }

    /// <summary>
    /// Compare two strings according to CurrentCultureIgnoreCase rules
    /// </summary>
    /// <param name="str">String to compare</param>
    /// <param name="compare">String to compare against</param>
    /// <returns>True if equal, false if not</returns>
    public static bool Like(this string str, string compare)
    {
      return string.Equals(str, compare, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Return a number of rightmost characters from a string
    /// </summary>
    /// <param name="str">String to operate on</param>
    /// <param name="length">Number of characters to extract</param>
    /// <returns>The new string</returns>
    public static string Right(this string str, int length)
    {
      return SafeSubstring(str, Math.Max(0, str.Length - length));
    }

    /// <summary>
    /// Same as Substring(), but will not throw exceptions if characters fall outside of the string bounds
    /// </summary>
    /// <param name="str">String to operate on</param>
    /// <param name="startIndex">Starting index</param>
    /// <returns>The new string</returns>
    public static string SafeSubstring(this string str, int startIndex)
    {
      return startIndex < str.Length ? str.Substring(startIndex) : "";
    }

    /// <summary>
    /// Same as Substring(), but will not throw exceptions if characters fall outside of the string bounds
    /// </summary>
    /// <param name="str">String to operate on</param>
    /// <param name="startIndex">Starting index</param>
    /// <param name="length">Number of characters from the starting index</param>
    /// <returns>The new string</returns>
    public static string SafeSubstring(this string str, int startIndex, int length)
    {
      var strlen = str.Length;

      if (startIndex >= strlen)
        return "";

      if (startIndex + length >= strlen)
        length = str.Length - startIndex;

      if (length <= 0)
        return "";

      return str.Substring(startIndex, length);
    }
  }
}
