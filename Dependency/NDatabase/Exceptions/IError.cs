namespace NDatabase.Exceptions
{
    internal interface IError
    {
        IError AddParameter<T>(T o) where T : class;

        IError AddParameter(string s);

        IError AddParameter(int i);

        IError AddParameter(byte i);

        IError AddParameter(long l);
    }
}
