using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace ParsingData
{
    public static class CommandParser
    {
        public static DataInfo Parse(ReadOnlySpan<char> text)
        {
           // var text = Encoding.UTF8.GetString(bytes).AsSpan();

            //если на вход ничего не пришло, возвращаем пустую структуру
            if (text.IsEmpty)
                return new DataInfo();

            var ind = text.IndexOf(' ');
            //если прислали только команду без параметров, то вроде как это тоже ошибка
            if (ind == -1)
                return new DataInfo();
            var command = text.Slice(0, ind);

            switch (command)
            {
                //команда, на которую приходит только value
                case "GET":
                    var key1 = text.Slice(ind + 1); ;
                    //если кроме ключа в GET передали что-то еще через пробел
                    if (key1.Contains(' '))
                        return new DataInfo();

                    return new DataInfo(command, key1, default);
                //команда с key и value
                case "SET":
                    text = text.Slice(ind + 1);
                    ind = text.IndexOf(' ');

                    //если в оставшейся части команды нет пробела
                    //т.е. передали только ключ без значения
                    if (ind == -1)
                        return new DataInfo();

                    var key = text.Slice(0, ind);

                    var value = text.Slice(ind + 1);
                    if (value.Contains(' '))
                        return new DataInfo();
                    return new DataInfo(command, key, value);
                default:
                    return new DataInfo();
            }
        }
    }
}
