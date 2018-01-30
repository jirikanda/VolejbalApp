import * as React from 'react';
import { ProductMatrixRow } from '../../components/productMatrix/ProductMatrixRow';
import { SelectedTopicPanel } from '../../components/common/SelectedTopicPanel';
import { Topic } from '../../apimodels/TopicsStore';
import { SatisfactionRateLevel, CustomAttribute, ProductVariant } from './../../apimodels/ProductMatrixStore';
import * as ProductMatrixStore from './../../store/ProductMatrix';

import { getAncestorsAndMe } from '../../helpers/hierarchy';
import { Table } from 'reactstrap';

interface ProductMatrixBodyProps {
    productVariants: ProductVariant[];
    productVariantsToHideByRateFilter: ProductVariant[];
    topics: Topic[];
    selectedTopics: Topic[];
    customAttributes: CustomAttribute[];
    unselectTopic: (topic: Topic) => void;
    satisfactionRateLevelFilters: ProductMatrixStore.SatisfactionRateLevelFilter[];
    isQuickSelection: boolean;
    currentSlideIndex: number;
    columnsToShow: number;
    hasPermissionsToEdit: boolean;
}

export const ProductMatrixBody = (props: ProductMatrixBodyProps) => {
    const { productVariants, topics, selectedTopics, customAttributes, unselectTopic, satisfactionRateLevelFilters, productVariantsToHideByRateFilter, isQuickSelection, hasPermissionsToEdit } = props;

    const filteredProductVariants: ProductVariant[] = productVariants
        .filter(productVariant => !productVariantsToHideByRateFilter.some(productVariantToHide => productVariantToHide.id === productVariant.id));

    return <div className="product-matrix-body">
        {
            selectedTopics.filter(topic => topic.children.length === 0).map(topic => {
                const rows = customAttributes
                    .filter(customAttribute => customAttribute.topicId === topic.id)
                    .map(customAttribute => <ProductMatrixRow
                        key={customAttribute.id}
                        customAttribute={customAttribute}
                        productVariants={filteredProductVariants}
                        satisfactionRateLevelFilters={satisfactionRateLevelFilters}
                        ancestorTopicsAndMe={getAncestorsAndMe(topic, topics)}
                        topic={topic}
                        currentSlideIndex={props.currentSlideIndex}
                        columnsToShow={props.columnsToShow}
                        hasPermissionsToEdit={hasPermissionsToEdit}/>);

                return <div key={topic.id}>
                    <Table className="product-matrix-body-selected-topic-panel">
                        <tbody><SelectedTopicPanel removeButtonClick={unselectTopic} topics={getAncestorsAndMe(topic, topics)} isQuickSelection={isQuickSelection} /></tbody>
                    </Table>
                    {rows}
                </div>
            })
        }
    </div>
}