import * as React from 'react';
import { ProductVariantDocument } from '../../../store/ProductVariantDocuments';
import * as classNames from 'classNames';

interface DocumentNameCellProps {
    productVariantDocument: ProductVariantDocument;
    isDragging?: boolean;
}

const DocumentNameCell = ({productVariantDocument, isDragging}: DocumentNameCellProps) =>
    <td>
        <a href={productVariantDocument.url}>
            <span className={classNames({"dragging-document": isDragging})}>
                {productVariantDocument.name}
            </span>
        </a>
    </td>

export default DocumentNameCell