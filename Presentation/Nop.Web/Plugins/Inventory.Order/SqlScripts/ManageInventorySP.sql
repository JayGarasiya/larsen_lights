CREATE OR ALTER PROCEDURE [dbo].[ManageInventorySP] 
(
        @Percentage DECIMAL = 0,
        @CategoryId INT = 0,
        @ManufacturerId INT = 0,
        @DateFrom NVARCHAR(MAX) = NULL,
        @DateTo NVARCHAR(MAX) = NULL
)
AS
BEGIN
DECLARE 
@sql nvarchar(MAX),
@orderQuery varchar(MAX)= ''
SET @sql = 'Select m.Id as ManufactureId,p.Id as ProductId, m.Name as Manufacture, 
p.Name,p.Sku SKU,p.ProductCost,MAX(p.StockQuantity) as InStock,
ISNULL(orderItem.Quantity,0) as QTY,
ISNULL(CEILING(orderItem.Quantity +(orderItem.Quantity) * '+ISNULL(CAST(@Percentage AS nvarchar(max)), '0') +' / 100.0),0) As Needed,
ISNULL(STUFF((SELECT '','' +  CAST(ISNULL(PoOrderItem.OrderedQty,0) as nvarchar)
              FROM PoOrder 
			  left join PoOrderItem  
			  on PoOrder.Id = PoOrderItem.PoOrderId 
              WHERE PoOrderItem.ProductId = p.Id and PoOrder.HasReceived = 0
			  order by PoOrder.CreatedOnUTC
              FOR XML PATH('''')),1,1,''''),''0,0,0'') as OrderedQty,
ISNULL(MAX(pd.DimensionsHeight * pd.DimensionsLength * pd.DimensionsWidth / 1000000),0) as BoxVolume,
ISNULL(MAX(pd.QtyCartoon),1) AS TotalCartoon
from Product p 

inner join Product_Manufacturer_Mapping pm on p.Id = pm.ProductId
inner join Manufacturer m on m.Id = pm.ManufacturerId and m.Deleted = 0
left join (
 select oi.ProductId,Sum(oi.Quantity) as Quantity,MAX(o.CreatedOnUtc) as CreatedOnUtc from OrderItem oi
 inner join [Order] o on oi.OrderId = o.Id #where
 group by oi.ProductId
 ) As orderItem on orderItem.ProductId = p.Id
left join Product_Category_Mapping pc on p.Id = pc.ProductId
left join ProductDimensions pd on pd.ProductId = p.Id'

SET @sql = @sql + ' WHERE p.Deleted = 0 and p.Published = 1 AND p.ManageInventoryMethodId = 1 AND p.Id NOT IN (SELECT DISTINCT ProductId FROM Product_ProductAttribute_Mapping)'

IF(@CategoryId > 0)
BEGIN
SET @sql = @sql +' AND pc.CategoryId in (select Id from Category where ParentCategoryId = '+ISNULL(CAST(@CategoryId AS nvarchar(max)), '0')+' or Id = '+ISNULL(CAST(@CategoryId AS nvarchar(max)), '0')+')'
END

IF(@ManufacturerId > 0)
BEGIN
SET @sql = @sql + 'AND pm.ManufacturerId = '+ISNULL(CAST(@ManufacturerId AS nvarchar(max)), '0') +''
END

IF(@DateFrom IS NOT NULL)
BEGIN
SET @sql = @sql +'AND orderItem.CreatedOnUtc >= '''+@DateFrom +''''
SET @orderQuery = 'where o.CreatedOnUtc >='''+@DateFrom +''''
END

IF(@DateTo IS NOT NULL)
BEGIN
SET @sql = @sql +'AND orderItem.CreatedOnUtc <= ''' + @DateTo +''''
IF(LEN(@orderQuery) > 0)
BEGIN
SET @orderQuery += 'and o.CreatedOnUtc <= ''' + @DateTo +''''
END
ELSE
BEGIN
SET @orderQuery += 'where o.CreatedOnUtc <= ''' + @DateTo +''''
END
END

SET @sql = @sql + 'GROUP BY p.Id,p.Sku,p.Name,m.Name,m.Id,p.Id,orderItem.Quantity ORDER BY m.Name,p.Sku'

SET @sql = REPLACE(@sql,'#where',@orderQuery)

print @sql
EXEC sp_executesql @sql
END