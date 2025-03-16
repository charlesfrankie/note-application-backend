using Microsoft.AspNetCore.Mvc;

namespace NoteApplication.Controllers.Requests
{
    public class NoteRequest
    {
        [FromQuery(Name = "per_page")]
        public int? PerPage { get; set; }
        [FromQuery(Name = "page")]
        public int? Page { get; set; }
        [FromQuery(Name = "order_by")]
        public string? OrderBy { get; set; }
        [FromQuery(Name = "filter_by")]
        public string? FilterBy { get; set; }
        [FromQuery(Name = "search")]
        public string? Search { get; set; }
    }
}