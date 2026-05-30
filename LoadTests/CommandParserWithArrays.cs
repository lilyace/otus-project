using ParsingData;
using System.Text;

namespace LoadTests
{
    public static class CommandParserWithArrays
    {
        public static DataInfo Parse(byte[] text)
        {
            //если на вход ничего не пришло, возвращаем пустую структуру
            if (text.Length == 0)
                return new DataInfo();
            var space = (byte)32;

            var ind = Array.IndexOf(text, space);
            //если прислали только команду без параметров, то вроде как это тоже ошибка
            if (ind == -1)
                return new DataInfo();
            var command = Encoding.UTF8.GetString(text.Take(ind).ToArray());

            switch (command)
            {
                //команда, на которую приходит только value
                case "GET":
                    var key1 = text.Skip(ind + 1).ToArray();
                    //если кроме ключа в GET передали что-то еще через пробел
                    if (key1.Contains(space))
                        return new DataInfo();

                    return new DataInfo(command, Encoding.UTF8.GetString(key1), default);
                //команда с key и value
                case "SET":
                    text = text.Skip(ind + 1).ToArray();
                    ind = Array.IndexOf(text, space);

                    //если в оставшейся части команды нет пробела
                    //т.е. передали только ключ без значения
                    if (ind == -1)
                        return new DataInfo();

                    var key = text.Take(ind).ToArray();

                    var value = text.Skip(ind + 1).ToArray();
                    if (value.Contains(space))
                        return new DataInfo();
                    return new DataInfo(command, Encoding.UTF8.GetString(key), value);
                case "DELETE":
                    var delKey = text.Skip(ind + 1).ToArray();
                    //если кроме ключа в DELETE передали что-то еще через пробел
                    if (delKey.Contains(space))
                        return new DataInfo();

                    return new DataInfo(command, Encoding.UTF8.GetString(delKey), default);
                default:
                    return new DataInfo();
            }
        }
    }
}
