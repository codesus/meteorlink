namespace MeteorLink
{
    public class MethodMessage
    {
        public dynamic Data { get; private set; }
        public string Collection { get; private set; }
        public string Id { get; private set; }
        public string Method { get; private set; }


        internal MethodMessage(string id, string method, string collection, dynamic data)
        {
            Id = id;
            Collection = collection;
            Data = data;
            Method = method;
        }


        public override string ToString()
        {
            return string.Format("Collection: {0} Id: {1} Method: {2} Data: {3}", this.Collection, this.Id, this.Method, this.Data);
        }

    }
}