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
        Task<(int Id, string DocumentNumber)> ConvertAndRegisterDocumentAsync(
      List<ConversionConsumedItemDto> consumedItems,
      List<ConversionProducedItemDto> producedItems);

        Task<bool> DeleteConversionDocumentAsync(int documentId);
        Task<string> GetNextConversionDocumentNumberAsync();
        Task<List<ConversionDocumentDto>> GetConversionDocumentsAsync();

        Task<(int Id, string DocumentNumber)> UpdateConversionDocumentAsync(
        int documentId,
        List<ConversionConsumedItemDto> consumedItems,
        List<ConversionProducedItemDto> producedItems,
        CancellationToken cancellationToken = default);
    }
}
