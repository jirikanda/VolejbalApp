import * as React from 'react';
import { ProductVariant } from '../../apimodels/ProductMatrixStore';
import * as moment from 'moment';
import UnselectProductVariant from '../../containers/products/UnselectProductVariant';

interface ProductMatrixHeaderItemProps {
    productVariant: ProductVariant
    onUnselectProductVariant: (productVariant: ProductVariant) => void;
    toggleProductVariantDocumentsDialog: () => void;
    requestProductVariantDocuments: (productVariant: ProductVariant) => void;
}

export const ProductMatrixHeaderItem = (props: ProductMatrixHeaderItemProps) => {
    const { productVariant, toggleProductVariantDocumentsDialog, requestProductVariantDocuments } = props;

    const handleFullMethodologyClick = (event: React.MouseEvent<HTMLElement>) => {
        event.preventDefault();
        requestProductVariantDocuments(productVariant);
        toggleProductVariantDocumentsDialog();
    }

    return <div className="product-matrix-header-item">
        <span className="float-left mr-2">
            <div className="product-item-img-placeholder"></div>
        </span>
        <span className="product-matrix-header-item-title">
            <b>{productVariant.name}</b>
        </span>

        <div className="clearfix"></div>
        <div className="mb-2">
            <a href="#" className="small mb-2" onClick={handleFullMethodologyClick}>
                <i className="fa fa-file-pdf-o"></i> kompletní metodiky
            </a>
            <UnselectProductVariant productVariant={productVariant} />
        </div>
        <div className="product-matrix-header-item-validfrom">
            {moment(productVariant.lastValidFrom).isValid() && <>
                {productVariant.wasRecentlyUpdated
                    ? <strong>{`aktualizováno ${moment(productVariant.lastValidFrom).format("l")}`}</strong>
                    : `aktualizováno ${moment(productVariant.lastValidFrom).format("l")}`
                }
            </>}
        </div>
    </div>
}