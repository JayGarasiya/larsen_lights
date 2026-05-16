namespace Nop.Plugin.Widgets.MakeTypeModel
{
    /// <summary>
    /// Represents plugin default values and constants
    /// </summary>
    public class MakeTypeModelDefaults
    {
        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "Widgets.MakeTypeModel";

        /// <summary>
        /// Gets the make product name
        /// </summary>
        public static string MakeProduct => "Widgets.MakeTypeModel.MakeProduct";

        /// <summary>
        /// Gets the type product name
        /// </summary>
        public static string TypeProduct => "Widgets.MakeTypeModel.TypeProduct";

        /// <summary>
        /// Gets the model product name
        /// </summary>
        public static string ModelProduct => "Widgets.MakeTypeModel.ModelProduct";

        /// <summary>
        /// Gets the model category name
        /// </summary>
        public static string ModelCategory => "Widgets.MakeTypeModel.ModelCategory";

        /// <summary>
        /// Gets the product imports name
        /// </summary>
        public static string ProductImports => "Widgets.MakeTypeModel.ProductImports";

        /// <summary>
        /// Gets the granit product imports name
        /// </summary>
        public static string GranitProductImports => "Widgets.MakeTypeModel.GranitProductImports";

        /// <summary>
        /// Gets the price imports name
        /// </summary>
        public static string PriceImports => "Widgets.MakeTypeModel.PriceImports";

        /// <summary>
        /// Gets the name of the view component widget zone into product detail pages
        /// </summary>
        public static string ProductDetailsAfterCollateral => "productdetails_after_collateral";

        /// <summary>
        /// Gets the stock update stored procedure
        /// </summary>
        public static string StockUpdateStoredProcedure = @"
        CREATE OR ALTER PROCEDURE [dbo].[UpdateProductStockFromJson]
            @JsonData NVARCHAR(MAX),           -- JSON from API (contains Payload array)
            @PriceRangeJson NVARCHAR(MAX)      -- JSON from PriceImport (PriceRange)
        AS
        BEGIN
            SET NOCOUNT ON;
        
            DECLARE @Payload NVARCHAR(MAX) = JSON_QUERY(@JsonData, '$.Payload');
        
            DECLARE @ProductData TABLE (
                SKU NVARCHAR(100),
                InStock NVARCHAR(10),
                DealerPrice DECIMAL(18, 4)
            );
        
            INSERT INTO @ProductData (SKU, InStock, DealerPrice)
            SELECT 
                LTRIM(RTRIM(JSON_VALUE(value, '$.HyCapStockNumber'))) AS SKU,
                JSON_VALUE(value, '$.InStock') AS InStock,
                TRY_CAST(JSON_VALUE(value, '$.DealerPrice') AS DECIMAL(18,4)) AS DealerPrice
            FROM OPENJSON(@Payload)
            WHERE JSON_VALUE(value, '$.HyCapStockNumber') IS NOT NULL;
        
            
            DECLARE @PriceRange TABLE (
                FromPrice DECIMAL(18, 2),
                ToPrice DECIMAL(18, 2),
                UsePercentage BIT,
                PricePercentage DECIMAL(18, 2),
                PriceAmount DECIMAL(18, 2)
            );
        
            INSERT INTO @PriceRange (FromPrice, ToPrice, UsePercentage, PricePercentage, PriceAmount)
            SELECT 
                TRY_CAST(JSON_VALUE(value, '$.FromPrice') AS DECIMAL(18, 2)),
                TRY_CAST(JSON_VALUE(value, '$.ToPrice') AS DECIMAL(18, 2)),
                TRY_CAST(JSON_VALUE(value, '$.UsePercentage') AS BIT),
                TRY_CAST(JSON_VALUE(value, '$.PricePercentage') AS DECIMAL(18, 2)),
                TRY_CAST(JSON_VALUE(value, '$.PriceAmount') AS DECIMAL(18, 2))
            FROM OPENJSON(@PriceRangeJson);
        
            DECLARE @MatchedProducts TABLE (
                ProductId INT,
                SKU NVARCHAR(100),
                OldQty INT,
                NewQty INT,
                OldCost DECIMAL(18, 4),
                NewCost DECIMAL(18, 4),
                OldPrice DECIMAL(18, 2),
                NewPrice DECIMAL(18, 2)
            );
        
            INSERT INTO @MatchedProducts (ProductId, SKU, OldQty, NewQty, OldCost, NewCost, OldPrice, NewPrice)
            SELECT 
                P.Id,
                P.Sku,
                P.StockQuantity,
                CASE WHEN PD.InStock = 'yes' THEN 1000 ELSE 0 END,
                P.ProductCost,
                PD.DealerPrice,
                P.Price,
                ROUND(
                    CASE 
                        WHEN PD.DealerPrice IS NULL OR PD.DealerPrice <= 0 THEN P.Price
                        ELSE
                            CASE 
                                WHEN R.FromPrice IS NOT NULL 
                                     AND PD.DealerPrice BETWEEN R.FromPrice AND R.ToPrice THEN
                                    CASE 
                                        WHEN R.UsePercentage = 1 
                                            THEN PD.DealerPrice + (PD.DealerPrice * R.PricePercentage / 100.0)
                                        ELSE PD.DealerPrice + R.PriceAmount
                                    END
                                ELSE 
                                    CASE 
                                        WHEN P.Price = 0 AND PD.DealerPrice > 0 THEN PD.DealerPrice
                                        ELSE P.Price
                                    END
                            END
                    END, 2
                ) AS NewPrice
            FROM Product P
            INNER JOIN @ProductData PD ON LTRIM(RTRIM(P.Sku)) = LTRIM(RTRIM(PD.SKU))
            OUTER APPLY (
                SELECT TOP 1 * 
                FROM @PriceRange R
                WHERE PD.DealerPrice BETWEEN R.FromPrice AND R.ToPrice
            ) R;
        
            UPDATE P
            SET 
                P.StockQuantity = MP.NewQty,
                P.ManageInventoryMethodId = 1,
                P.ProductCost = MP.NewCost,
                P.Price = MP.NewPrice,
                P.UpdatedOnUtc = GETUTCDATE()
            FROM Product P
            INNER JOIN @MatchedProducts MP ON P.Id = MP.ProductId
            WHERE 
                (MP.OldQty <> MP.NewQty)
                OR (MP.OldCost <> MP.NewCost)
                OR (MP.OldPrice <> MP.NewPrice);
        
            IF OBJECT_ID('dbo.StockQuantityHistory', 'U') IS NOT NULL
            BEGIN
                INSERT INTO StockQuantityHistory (
                    ProductId,
                    QuantityAdjustment,
                    StockQuantity,
                    Message,
                    CreatedOnUtc
                )
                SELECT 
                    MP.ProductId,
                    MP.NewQty - MP.OldQty,
                    MP.NewQty,
                    'Stock updated by Hy-Cap Stock & Price API',
                    GETUTCDATE()
                FROM @MatchedProducts MP
                WHERE MP.OldQty <> MP.NewQty;
            END
        
            IF OBJECT_ID('dbo.ProductPriceHistory', 'U') IS NOT NULL
            BEGIN
                INSERT INTO ProductPriceHistory (
                    ProductId,
                    OldPrice,
                    NewPrice,
                    OldCost,
                    NewCost,
                    Message,
                    CreatedOnUtc
                )
                SELECT 
                    MP.ProductId,
                    MP.OldPrice,
                    MP.NewPrice,
                    MP.OldCost,
                    MP.NewCost,
                    'Price updated by Hy-Cap Stock & Price API',
                    GETUTCDATE()
                FROM @MatchedProducts MP
                WHERE (MP.OldCost <> MP.NewCost) OR (MP.OldPrice <> MP.NewPrice);
            END
        END
        ";

        /// <summary>
        /// Return Request Note
        /// </summary>
        public const string ReturnRequestNote = "ReturnRequestNote";

        /// <summary>
        /// Return Request Note additional token
        /// </summary>
        public const string ReturnRequestNoteToken = "ReturnRequest.Note";

    }
}