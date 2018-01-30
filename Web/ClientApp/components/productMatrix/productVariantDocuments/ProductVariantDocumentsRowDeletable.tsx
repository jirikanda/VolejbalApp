import * as React from 'react';
import { ProductVariantDocument } from '../../../store/ProductVariantDocuments';
import DeletableRowContent from './DeletableRowContent';
import { DragSource, DropTarget } from 'react-dnd';
import DocumentNameCell from './DocumentNameCell';

export interface ProductVariantDocumentsRowDeletableProps {
    productVariantDocument: ProductVariantDocument;
    deleteProductVariantDocuments: (productVariantDocument: ProductVariantDocument) => void;
}

const ProductVariantDocumentsRowDeletable = (props: ProductVariantDocumentsRowDeletableProps) =>
    <tr>
        <DocumentNameCell productVariantDocument={props.productVariantDocument} />
        <DeletableRowContent {...props} key={props.productVariantDocument.id} />
    </tr>

export default ProductVariantDocumentsRowDeletable;