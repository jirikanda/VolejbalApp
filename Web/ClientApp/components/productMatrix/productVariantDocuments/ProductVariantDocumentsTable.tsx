import * as React from 'react';
import { Button, Table  } from 'reactstrap';
import Modal from '../../common/Modal';
import { ProductVariantDocument, ProductVariantDocumentFile } from '../../../store/ProductVariantDocuments';
import * as moment from 'moment';
import ReadOnlyRowContent from './ReadOnlyRowContent';
import EditableRowContent from './EditableRowContent';
import DeletableRowContent from './DeletableRowContent';
import ProductVariantDocumentFileUpload from './ProductVariantDocumentsFileUpload';
import { DragSource, DropTarget } from 'react-dnd';
import { targetCollector, sourceCollector, itemSourceSpec, itemTargetSpec, ItemTypes, ItemAndDocumentComponentProps } from './ProductVariantDocumentsDragAndDrop';
import ProductVariantDocumentsRow from './ProductVariantDocumentsRow';
import ProductVariantDocumentsRowEditable from './ProductVariantDocumentsRowEditable';
import ProductVariantDocumentsRowDeletable from './ProductVariantDocumentsRowDeletable';

export interface ProductVariantDocumentsTableProps {
    productVariantDocuments: ProductVariantDocument[];
    deleteProductVariantDocuments: (productVariantDocument: ProductVariantDocument) => void;
    saveProductVariantDocumentsMetadata: (productVariantDocument: ProductVariantDocument, newValidFrom: Date, newValidTo?: Date) => void;
    changeProductVariantDocumentsOrder: (hoverIndex: number, currentItemId: number) => void;
    addProductVariantDocumentFile: (newDocument: ProductVariantDocumentFile) => void;
    editable?: boolean;
    deletable?: boolean;
}

const ProductVariantDocumentsTable = (props: ProductVariantDocumentsTableProps) => {
    const {productVariantDocuments, editable, deletable, deleteProductVariantDocuments} = props;
    let rows;

    if(editable) {
        rows = productVariantDocuments.map((document, index) =>
            <ProductVariantDocumentsRowEditable
                {...props}
                key={document.id}
                index={index}
                id={document.id}
                productVariantDocument={document} />
        );
    }
    else if(deletable) {
        rows = productVariantDocuments.map((document, index) =>
            <ProductVariantDocumentsRowDeletable
                {...props}
                key={document.id}
                productVariantDocument={document} />
        );
    }
    else {
        rows = productVariantDocuments.map((document, index) =>
            <ProductVariantDocumentsRow
                key={document.id}
                productVariantDocument={document} />
        );
    }

    return <Table bordered size="sm">
        <thead>
            <tr>
                <th></th>
                <th>Platnost</th>
            </tr>
        </thead>
        <tbody>
            {editable && <ProductVariantDocumentFileUpload {...props} />}
            {rows}
        </tbody>
    </Table>
}

// FIXME: PK: we have to fix "any", but I can't see the way right now
export default ProductVariantDocumentsTable