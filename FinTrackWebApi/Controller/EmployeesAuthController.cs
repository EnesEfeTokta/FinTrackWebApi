using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos;
using FinTrackWebApi.Models;
using FinTrackWebApi.Security;
using FinTrackWebApi.Services.EmailService;
using FinTrackWebApi.Services.OtpService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesAuthController : ControllerBase
    {
    }
}
