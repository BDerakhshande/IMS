using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IConversionService
    {
        Task<int> ConvertAndRegisterDocumentAsync(
    List<ConversionConsumedItemDto> consumedItems,
    List<ConversionProducedItemDto> producedItems);

        Task<bool> DeleteConversionDocumentAsync(int documentId);

        Task<List<ConversionDocumentDto>> GetConversionDocumentsAsync();


    }
}
