using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            // для 404 — отдельная страница
            if (statusCode == 404)
                return View("NotFoundMemasik");

            // можно добавить другие, если нужно
            if (statusCode == 403)
                return View("Forbidden");

            // всё остальное – общая
            ViewBag.StatusCode = statusCode;
            return View("Generic");
        }

        [Route("Error/500")]
        public IActionResult ServerError()
        {
            return View("ServerError");
        }
    }
}
