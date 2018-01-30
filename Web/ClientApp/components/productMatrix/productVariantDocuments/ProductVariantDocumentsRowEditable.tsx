import * as React from 'react';
import { Button, Table  } from 'reactstrap';
import Modal from '../../common/Modal';
import { ProductVariantDocument } from '../../../store/ProductVariantDocuments';
import * as moment from 'moment';
import EditableRowContent from './EditableRowContent';
import { DragSource, DropTarget } from 'react-dnd';
import { targetCollector, sourceCollector, itemSourceSpec, itemTargetSpec, ItemTypes, ItemAndDocumentComponentProps } from './ProductVariantDocumentsDragAndDrop';
import DocumentNameCell from './DocumentNameCell';

export interface ProductVariantDocumentsRowEditableProps {
    productVariantDocument: ProductVariantDocument;
    deleteProductVariantDocuments: (productVariantDocument: ProductVariantDocument) => void;
    saveProductVariantDocumentsMetadata: (productVariantDocument: ProductVariantDocument, newValidFrom: Date, newValidTo?: Date) => void;
    changeProductVariantDocumentsOrder: (hoverIndex: number, currentItemId: number) => void;
    editable?: boolean;
    deletable?: boolean;
}

class ProductVariantDocumentsRowEditable extends React.Component<ItemAndDocumentComponentProps> {
    render() {
        const {productVariantDocument, connectDragSource, connectDropTarget, isDragging} = this.props;

        return connectDragSource && connectDropTarget
            ? connectDragSource(
                connectDropTarget(
                    <tr>
                        <DocumentNameCell
                            isDragging={isDragging}
                            productVariantDocument={productVariantDocument} />
                        <EditableRowContent
                            {...this.props}
                            key={productVariantDocument.id}
                            productVariantDocument={productVariantDocument} />
                    </tr>
                )
            )
            : <tr/>
    }
}

// FIXME: PK: we have to fix "any", but I can't see the way right now
export default DragSource(ItemTypes.ProductVariantDocument, itemSourceSpec, sourceCollector)(DropTarget(
    ItemTypes.ProductVariantDocument, itemTargetSpec, targetCollector)(ProductVariantDocumentsRowEditable as any));