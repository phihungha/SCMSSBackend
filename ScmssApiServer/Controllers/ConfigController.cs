﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScmssApiServer.DTOs;
using ScmssApiServer.IDomainServices;
using ScmssApiServer.Models;
using ScmssApiServer.Utilities;

namespace ScmssApiServer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : CustomControllerBase
    {
        private readonly IConfigService _configService;

        public ConfigController(IConfigService configService, IUsersService usersService)
            : base(usersService)
        {
            _configService = configService;
        }

        [HttpGet]
        public async Task<ActionResult<Config>> Get()
        {
            return await _configService.GetAsync();
        }

        [HttpPatch]
        public async Task<ActionResult<Config>> Set(ConfigInputDto body)
        {
            return await _configService.SetAsync(body);
        }
    }
}