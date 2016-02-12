using System;

namespace Cronical.Misc
{
  public class Crc32
  {
    private const uint Polynomial = 0xEDB88320;
    private static readonly uint[] Table = new uint[256];

    static Crc32()
    {
      for (uint i = 0; i < Table.Length; ++i)
      {
        uint temp = i;

        for (int j = 8; j > 0; --j)
          if ((temp & 1) == 1)
            temp = (temp >> 1) ^ Polynomial;
          else
            temp >>= 1;

        Table[i] = temp;
      }
    }

    /// <summary>
    /// Compute a crc-32 checksum as an integer.
    /// </summary>
    /// <param name="bytes">The bytes to compute the checksum from</param>
    /// <returns>A signed integer containing the crc-32 value</returns>
    public static int ComputeChecksum(byte[] bytes)
    {
      uint crc = 0xffffffff;

      foreach (byte t in bytes)
        crc = (crc >> 8) ^ Table[(byte)((crc & 0xff) ^ t)];

      return (int)~crc;
    }

    /// <summary>
    /// Compute a crc-32 checksum as a byte[].
    /// </summary>
    /// <param name="bytes">The bytes to compute the checksum from</param>
    /// <returns>A byte[4] containing the crc-32 value</returns>
    public static byte[] ComputeChecksumBytes(byte[] bytes)
    {
      return BitConverter.GetBytes(ComputeChecksum(bytes));
    }

    /// <summary>
    /// Compute a crc-32 checksum as a string of hexadecimal values.
    /// </summary>
    /// <param name="bytes">The bytes to compute the checksum from</param>
    /// <returns>A string containing the hexadecimal representation of the crc-32 value</returns>
    public static string ComputeChecksumString(byte[] bytes)
    {
      return ComputeChecksum(bytes).ToString("X4");
    }
  }
}
