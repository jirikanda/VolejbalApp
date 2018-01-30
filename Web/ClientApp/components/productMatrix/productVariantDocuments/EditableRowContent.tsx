import * as React from 'react';

import { getInputDateFormat, getDateDiff } from '../../../helpers/momentHelpers';
import { ProductVariantDocument } from '../../../store/ProductVariantDocuments';
import DeleteButton from './DeleteButton';
import { Toastr } from '../../../store/commonActionCreators/Toaster';

interface EditableRowContentProps {
    productVariantDocument: ProductVariantDocument;
    deleteProductVariantDocuments: (productVariantDocument: ProductVariantDocument) => void;
    saveProductVariantDocumentsMetadata: (productVariantDocument: ProductVariantDocument, newValidFrom: Date, newValidTo?: Date) => void;
}

const EditableRowContent = (props: EditableRowContentProps) => {
    let validFromInputRef: HTMLInputElement;
    let validToInputRef: HTMLInputElement;
    const { productVariantDocument, saveProductVariantDocumentsMetadata } = props;

    const handleValidFromInputChange = () => {
        saveProductVariantDocumentsMetadata(productVariantDocument, validFromInputRef.valueAsDate, validToInputRef.valueAsDate);
    }

    const handleValidToInputChange = () => {
        if(validToInputRef.valueAsDate && getDateDiff(validFromInputRef.valueAsDate, validToInputRef.valueAsDate) > 0) {
            Toastr.error({message: 'Datum Od je větší než datum Do'});
            validToInputRef.value = '';
            return;
        }
        saveProductVariantDocumentsMetadata(productVariantDocument, validFromInputRef.valueAsDate, validToInputRef.valueAsDate);
    }

    return <td>
        <span className="mr-1">
            Od:
        </span>
        <input type="date"
            onChange={handleValidFromInputChange}
            ref={(ref) => validFromInputRef = ref}
            value={getInputDateFormat(productVariantDocument.validFrom)} />
        {" "}
        <span className="mr-1">
            Do:
        </span>
        <input type="date"
            onChange={handleValidToInputChange}
            ref={(ref) => validToInputRef = ref}
            value={productVariantDocument.validTo && getInputDateFormat(productVariantDocument.validTo)} />

        <DeleteButton {...props} />
    </td>
}

export default EditableRowContent