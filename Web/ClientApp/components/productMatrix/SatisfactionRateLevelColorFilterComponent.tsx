import * as React from 'react';
import * as classnames from 'classnames';
import { SatisfactionRateLevel, CustomAttribute, ProductVariant } from '../../apimodels/ProductMatrixStore';
import * as ProductMatrixStore from '../../store/ProductMatrix';

interface SatisfactionRateLevelColorFilterComponentProps {
    satisfactionRateLevels: SatisfactionRateLevel[];
    satisfactionRateLevelFilters: ProductMatrixStore.SatisfactionRateLevelFilter[];
    customAttribute: CustomAttribute;
    toggleSatisfactionRateLevel: (rateLevel: SatisfactionRateLevel, customAttribute: CustomAttribute) => void;
}

export default class SatisfactionRateLevelColorFilterComponent extends React.Component<SatisfactionRateLevelColorFilterComponentProps> {
    isFiltered = (rateLevel: SatisfactionRateLevel) => {
        return this.props.satisfactionRateLevelFilters.some(filter =>
            (filter.customAttribute.id === this.props.customAttribute.id)
            && (filter.satisfactionRateLevels.some(filteredLevel => filteredLevel.cssClass === rateLevel.cssClass)));
    }

    render() {
        const { satisfactionRateLevels, toggleSatisfactionRateLevel, customAttribute} = this.props;

        return <div className="satisfaction-rate-level-container">
            {satisfactionRateLevels.map(rateLevel =>
                <div
                    key={rateLevel.cssClass}
                    onClick={event => toggleSatisfactionRateLevel(rateLevel, customAttribute) }
                    className={classnames("satisfaction-rate-level", rateLevel.cssClass, { "crossed": this.isFiltered(rateLevel) })}>
                </div>)}
        </div>;
    }
}