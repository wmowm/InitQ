using InitQ.Cache;
using InitQ.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InitQWebApiTest.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        private readonly ICacheService _redisService;

        public DefaultController(ICacheService redisService) 
        {
            _redisService = redisService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //长期执行任务
            await _redisService.ListLeftPushAsync("tibos_test_1", "测试长期执行任务");

            //延迟执行任务(延迟3s后执行)
            var dt = DateTime.Now.AddSeconds(3);
            await _redisService.SortedSetAddAsync("test_dalay_test_1", $"测试延迟执行任务,预计执行时间:{dt.ToString("yyyy-MM-dd HH:mm:ss")}", dt);

            //循环执行任务
            await _redisService.RemoveAsync("tibos_interval_test_count");
            await _redisService.ListLeftPushAsync("tibos_interval_test_1", new IntervalMessage() { Msg = "测试循环执行任务" });
            return Ok("200");
        }

        [HttpGet]
        public async Task<IActionResult> GetCount() 
        {
            var res = await _redisService.GetAsync("tibos_interval_test_count");
            return Ok(res);
        }
    }
}
