namespace TreeListControl.Tree
{
    /// <summary>
    ///   Simple class to display an error message
    /// </summary>
    public class ErrorMessage
    {
        #region Public Properties

        public bool IsXmlElement { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public object Tag { get; set; }

        #endregion Public Properties
    }

}
