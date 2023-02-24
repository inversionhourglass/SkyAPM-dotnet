namespace SkyApm.Config
{
    [Config("SkyWalking", "Spanable")]
    public class SpanableConfig
    {
        /// <summary>
        /// If the segment has incomplete spans, how long to wait for the longest, and then the segment will be forcibly complete.<br/>
        /// If your oap server is using the official mirror, then please set this value to 0, or the segment's duration will be wrong, 
        /// I have asked the official whether the logic can be adjusted, but no response so far: 
        /// https://github.com/apache/skywalking/discussions/10371 <br/>
        /// </summary>
        public int DelaySeconds { get; set; } = 0;

        public int DelayInspectInterval { get; set; } = 3000;

        /// <summary>
        /// prefix: add <see cref="IncompletePrefix"/> as a prefix to OperationName.(default)<br/>
        /// error: set span's <see cref="Tracing.Segments.SegmentSpan.IsError"/> to true.<br/>
        /// tag: only add a tag to the span, other options also will add the tag.<br/>
        /// </summary>
        public ScenarioSymbol IncompleteSymbol { get; set; } = ScenarioSymbol.Prefix;

        /// <summary>
        /// if <see cref="IncompleteSymbol"/> is prefix, this will be applied to OperationName as a prefix.
        /// </summary>
        public string IncompletePrefix { get; set; } = "[incomplete]";

        public ScenarioSymbol AsyncLinkSymbol { get; set; } = ScenarioSymbol.Prefix;

        public string AsyncLinkPrefix { get; set; } = "[alink]";
    }

    public enum ScenarioSymbol
    {
        Prefix,
        Error,
        Tag
    }
}
