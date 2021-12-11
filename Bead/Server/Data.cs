using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{

    enum Command
    {
        Login,
        Logout,
        Message,
        List,
        Accept,
        Decline,
        Null,
        Heat,
        Cool
    }


    class Data
    {
        public Command cmdCommand;
        public string strName;
        public string strMessage;

        public Data()
        {
            this.cmdCommand = Command.Null;
            this.strName = null;
            this.strMessage = null;

        }

        public Data(byte[] data)
        {
            this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);
            int nameLength = BitConverter.ToInt32(data, 4);
            int messageLength = BitConverter.ToInt32(data, 8);

            if(nameLength > 0)
            {
                this.strName = Encoding.Default.GetString(data, 12, nameLength);
            }
            else
            {
                this.strName = null;
            }

            if(messageLength > 0)
            {
                this.strMessage = Encoding.Default.GetString(data, 12+nameLength, messageLength);
            }
            else
            {
                this.strMessage = null;
            }

        }

        public byte[] toByte()
        {
            List<byte> result = new List<byte>();

            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            if(strName != null)
            {
                result.AddRange(BitConverter.GetBytes(strName.Length));
            }
            else
            {
                result.AddRange(BitConverter.GetBytes(0));
            }

            if(strMessage != null)
            {
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            }
            else
            {
                result.AddRange(BitConverter.GetBytes(0));
            }
            if(strName != null)
            {
                result.AddRange(Encoding.Default.GetBytes(strName));

            }
            if(strMessage != null)
            {
                result.AddRange(Encoding.Default.GetBytes(strMessage));
            }


            return result.ToArray();
        }


    }
}
