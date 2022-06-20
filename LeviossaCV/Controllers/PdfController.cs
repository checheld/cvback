﻿using Domain;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Abstract;

namespace LeviossaCV.Controllers
{
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly IPdfService _PdfService;
        public PdfController(IServiceProvider _serviceProvider)
        {
            _PdfService = _serviceProvider.GetService<IPdfService>();
        }

        [HttpGet]
        [Route("pdf/{id}")]
        public async Task<IActionResult> GetPdfById(int id)
        {
            try
            {
                return Ok(await _PdfService.GetPdf(id));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}