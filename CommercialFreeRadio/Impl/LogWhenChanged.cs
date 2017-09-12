
namespace CommercialFreeRadio.Impl
{
    public class LogWhenChanged
    {
        private string lastLogged;
        public void Info(string message)
        {
            if( message != lastLogged )
                Logger.Info(message);
            lastLogged = message;
        }
    }
}
