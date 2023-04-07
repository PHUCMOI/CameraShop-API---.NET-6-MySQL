using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CameraAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CameraAPI.Services.Interfaces;
using CameraAPI.Repositories;
using CameraService.Services.IRepositoryServices;

namespace CameraAPI.Controllers
{
    [Route("api/category")]
    [ApiController, Authorize]
    public class CategoriesController : ControllerBase
    {
        public readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategory()
        {
            var categories = await _categoryService.GetAllCategory();
            if (categories == null)
            {
                return NotFound();
            }
            return Ok(categories);
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var categories = await _categoryService.GetIdAsync(id);
            if (categories != null)
            {
                return Ok(categories);
            }
            return BadRequest();
        }

        // PUT: api/Categories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (category != null)
            {
                var categories = await _categoryService.Update(category);
                if (categories)
                {
                    return Ok(categories);
                }
            }
            return BadRequest();
        }

        // POST: api/Categories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            var categories = await _categoryService.Create(category);
            if (categories)
            {
                return Ok(categories);
            }
            return BadRequest();
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var categories = await _categoryService.DeleteAsync(id);
            if (categories)
            {
                return Ok(categories);
            }
            return BadRequest();
        }
    }
}
