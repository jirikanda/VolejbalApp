import * as React from 'react';
import { ProductVariantDocument } from '../../../store/ProductVariantDocuments';
import ReadOnlyRowContent from './ReadOnlyRowContent';
import { DragSource, DropTarget } from 'react-dnd';
import { targetCollector, sourceCollector, itemSourceSpec, itemTargetSpec, ItemTypes, ItemAndDocumentComponentProps } from './ProductVariantDocumentsDragAndDrop';
import * as classNames from 'classNames';
import DocumentNameCell from './DocumentNameCell';

export interface ProductVariantDocumentsRowProps {
    productVariantDocument: ProductVariantDocument;
}

const ProductVariantDocumentsRow = ({productVariantDocument}: ProductVariantDocumentsRowProps) =>
    <tr>
        <DocumentNameCell
            productVariantDocument={productVariantDocument} />
        <ReadOnlyRowContent
            key={productVariantDocument.id}
            productVariantDocument={productVariantDocument} />
    </tr>

export default ProductVariantDocumentsRow;