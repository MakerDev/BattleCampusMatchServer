using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampusMatchServer.Services
{
    public class Utils
    {
        public static string GenerateMatchID()
        {
            string id = string.Empty;

            for (int i = 0; i < 5; i++)
            {
                var rand = new Random();
                int random = rand.Next(0, 36);
                if (random < 26)
                {
                    id += (char)(random + 65);
                }
                else
                {
                    id += (random - 26).ToString();
                }
            }

            return id;
        }
    }
}
