import * as React from 'react';
import { Input, Button } from 'reactstrap';
import { ProductVariantDocument, ProductVariantDocumentFile } from '../../../store/ProductVariantDocuments';
import { getInputDateFormat } from '../../../helpers/momentHelpers';
import { Toastr } from '../../../store/commonActionCreators/Toaster';

interface ProductVariantDocumentFileUploadProps {
    addProductVariantDocumentFile: (newDocument: ProductVariantDocumentFile) => void;
}

const ProductVariantDocumentFileUpload = ({addProductVariantDocumentFile}: ProductVariantDocumentFileUploadProps) => {
    let validFromInputRef: HTMLInputElement;
    let validToInputRef: HTMLInputElement;
    let fileInputRef: HTMLInputElement;
    let file: File;

    const addDocument = () => {
        addProductVariantDocumentFile({
            file,
            validFrom: validFromInputRef.valueAsDate,
            validTo: validToInputRef.valueAsDate
        });
    }

    const resetInputsValues = () => {
        fileInputRef.value = "";
        validFromInputRef.value = "";
        validToInputRef.value = "";
    }

    const handleFileUpload = () => {
        file = fileInputRef.files.item(0);
    }

    const handleAddFileClick = () => {
        if(validFromInputRef.valueAsDate === null) {
            Toastr.error({message: "Je nutné zadat datum Od"});
            return;
        }

        if(file === undefined) {
            Toastr.error({message: "Je nutné vybrat soubor"});
            return;
        }

        addDocument();
        resetInputsValues();
    }

    return <tr>
        <td>
            <input onChange={handleFileUpload} ref={(ref) => fileInputRef = ref} type="file" />
        </td>
        <td>
            Od: <input
                ref={(ref) => validFromInputRef = ref}
                type="date" />
            {" "}
            Do: <input
                ref={(ref) => validToInputRef = ref}
                type="date" />
            <Button
                color="primary"
                outline
                className="pull-right"
                onClick={handleAddFileClick}>
                Přidat soubor
            </Button>
        </td>
    </tr>
}

export default ProductVariantDocumentFileUpload