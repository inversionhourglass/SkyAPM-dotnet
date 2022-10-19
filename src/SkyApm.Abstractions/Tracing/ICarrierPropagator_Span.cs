using SkyApm.Tracing.Segments;

namespace SkyApm.Tracing
{
    partial interface ICarrierPropagator
    {
        void Inject(SegmentSpan span, ICarrierHeaderCollection carrier);
    }
}
