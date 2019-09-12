namespace IUBH.TOR.Data
{
    internal static class DataDependencies
    {
        public static void Register()
        {
            var container = TinyIoC.TinyIoCContainer.Current;

            container.Register<IDbConnection>((x, y) => DbConnection.Instance);
            container.Register<IUnitOfWork, SqliteUnitOfWork>();
        }
    }
}
