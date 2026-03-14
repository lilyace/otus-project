using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace ParsingData
{
    public static class CommandParser
    {
        public static DataInfo Parse(Span<byte> textSpan)
        {
            //если на вход ничего не пришло, возвращаем пустую структуру
            if (textSpan.IsEmpty)
                return new DataInfo();
            var space = (byte)32;
           // var text = Encoding.UTF8.GetString(bytes).AsSpan();

            var ind = textSpan.IndexOf(space);
            //если прислали только команду без параметров, то вроде как это тоже ошибка
            if (ind == -1)
                return new DataInfo();
            var command = Encoding.UTF8.GetString(textSpan.Slice(0, ind));

            switch (command)
            {
                //команда, на которую приходит только value
                case "GET":
                    var key1 = textSpan.Slice(ind + 1); ;
                    //если кроме ключа в GET передали что-то еще через пробел
                    if (key1.Contains(space))
                        return new DataInfo();

                    return new DataInfo(command, Encoding.UTF8.GetString(key1), default);
                //команда с key и value
                case "SET":
                    textSpan = textSpan.Slice(ind + 1);
                    ind = textSpan.IndexOf(space);

                    //если в оставшейся части команды нет пробела
                    //т.е. передали только ключ без значения
                    if (ind == -1)
                        return new DataInfo();

                    var key = textSpan.Slice(0, ind);

                    var value = textSpan.Slice(ind + 1);
                    if (value.Contains(space))
                        return new DataInfo();
                    return new DataInfo(command, Encoding.UTF8.GetString(key), value);
                default:
                    return new DataInfo();
            }
        }
    }
}
