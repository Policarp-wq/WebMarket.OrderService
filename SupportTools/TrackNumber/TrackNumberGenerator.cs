using System.Text;

namespace WebMarket.OrderService.SupportTools.TrackNumber
{
    public class TrackNumberGenerator : ITrackNumberGenerator
    {
        //https://ru.wikipedia.org/wiki/%D0%9F%D0%BE%D1%87%D1%82%D0%BE%D0%B2%D1%8B%D0%B9_%D0%B8%D0%B4%D0%B5%D0%BD%D1%82%D0%B8%D1%84%D0%B8%D0%BA%D0%B0%D1%82%D0%BE%D1%80
        private const int RANDOM_PART_LENGTH = 6;
        private const string ALPHABET = "0123456789abcdefghijklmnopqrstuvwxyz";
        private static Random _random = new Random();
        //[random][month][check_num] absolute length is 9
        public string GenerateTrackNumber()
        {
            StringBuilder sb = new StringBuilder();
            int checkNum = 0;
            for(int i = 0; i < RANDOM_PART_LENGTH; i++)
            {
                int random = _random.Next(ALPHABET.Length);
                sb.Append(ALPHABET[random]);
                if (i % 2 == 0)
                    checkNum += random;
            }
            var month = DateTime.Now.Month;
            sb.Append(month > 9 ? month: "0" + month);
            sb.Append(checkNum % 10);
            return sb.ToString();
        }

    }
}
