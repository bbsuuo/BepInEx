using System;

namespace BepInEx.Preloader.Socket
{
    public class Package
    {
        public byte Key { get; set; }
        public byte[] Body { get; set; }

        public Package(byte key, byte[] body)
        {
            Key = key;
            Body = body;
        }

        public byte[] ToByteArray()
        {
            byte[] packageBytes = new byte[3 + Body.Length];

            packageBytes[0] = Key;
            BitConverter.GetBytes((short)Body.Length).CopyTo(packageBytes, 1);
            Body.CopyTo(packageBytes, 3);

            return packageBytes;
        }

        public static Package FromByteArray(byte[] data)
        {
            byte key = data[0];
            short bodyLength = BitConverter.ToInt16(data, 1);
            byte[] body = new byte[bodyLength];
            Buffer.BlockCopy(data, 3, body, 0, bodyLength);

            return new Package(key, body);
        }
    }

}
