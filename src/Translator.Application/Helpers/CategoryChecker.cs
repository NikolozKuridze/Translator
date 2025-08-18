using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Helpers;

public class CategoryChecker(IRepository<Category> _categoryRepository)
{
    public async Task CheckAncestorsAsync(string type, Guid? parentId, CancellationToken cancellationToken)
    {
        var tempParentId = parentId;

        while (tempParentId != null)
        {
            var parent = await _categoryRepository
                .AsQueryable()
                .FirstOrDefaultAsync(p => p.Id == tempParentId, cancellationToken);
            
            if (parent is null)
                throw new CategoryNotFoundException(tempParentId);
            
            if (type.Equals(parent.Type, StringComparison.OrdinalIgnoreCase))
                throw new CategoryAlreadyExistsException();

            tempParentId = parent.ParentId;
        }
    }

    public async Task CheckSiblingsAsync(string value, string type, Guid? parentId, Guid? currentCategoryId,
        CancellationToken cancellationToken)
    {
      var siblingsQuery = _categoryRepository.AsQueryable();
      
      
      siblingsQuery = siblingsQuery.Where(c
          => (parentId.HasValue && c.ParentId == parentId) 
             || c.ParentId == null); 
       
      var siblingWithConflict = await siblingsQuery
          .FirstOrDefaultAsync(c => (currentCategoryId == null || c.Id != currentCategoryId) &&
                                    c.Value.ToLower() == value.ToLower() &&
                                    c.Type.ToLower() == type.ToLower(),
              cancellationToken);
      
      if(siblingWithConflict != null)
          throw new CategoryAlreadyExistsException();
    }
}