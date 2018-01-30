import * as React from 'react';
import { Tooltip } from 'reactstrap';

import { Topic } from 'ClientApp/apimodels/TopicsStore';
import { SearchTopicsItem } from 'ClientApp/store/Topics';

interface TopicSearchResultItemProps {
    searchTopicsItem: SearchTopicsItem;
    unselectFoundTopic: (topic: Topic) => void;
    selectFoundTopic: (topic: Topic) => void;
    customAttributeAndCustomAttributeValueOccurencesCount: number;
    isSearchAnswersModeOn: boolean;
}

interface TopicSearchResultItemState {
    isTooltipOpen: boolean;
}

class TopicSearchResultItem extends React.Component<TopicSearchResultItemProps, TopicSearchResultItemState> {
    checkBoxRef: HTMLInputElement;

    constructor(props) {
        super(props);

        this.state = {
          isTooltipOpen: false
        };
    }

    getSelectionControlFunction = (isCheckedOrIndeterminate: boolean) => isCheckedOrIndeterminate ? this.props.unselectFoundTopic : this.props.selectFoundTopic;

    setCheckBoxState = (checkbox: HTMLInputElement) => {
        if (checkbox) {
            const {searchTopicsItem} = this.props;

            this.checkBoxRef = checkbox;
            checkbox.checked = searchTopicsItem.isChecked;
            checkbox.indeterminate = searchTopicsItem.isIndeterminate;
        }
    }

    handleCheckboxClick = (event: React.MouseEvent<HTMLLIElement | HTMLInputElement>) => {
        const {searchTopicsItem} = this.props;

        this.getSelectionControlFunction(searchTopicsItem.isChecked || searchTopicsItem.isIndeterminate)(searchTopicsItem as Topic);
    }

    handleToggleTooltip = () => {
        this.setState((prevState) => ({
          isTooltipOpen: !prevState.isTooltipOpen
        }));
      }

    render() {
        const {searchTopicsItem, customAttributeAndCustomAttributeValueOccurencesCount, isSearchAnswersModeOn} = this.props;

        return <li className="topic-search-result-item">
            <span>
                <input
                    className="pointer"
                    type="checkbox"
                    onClick={this.handleCheckboxClick}
                    ref={this.setCheckBoxState} />
                {" "}
                <label>
                {searchTopicsItem.displayName}
                </label>
            </span>
            {
                isSearchAnswersModeOn && customAttributeAndCustomAttributeValueOccurencesCount > 0
                    && <span className="pull-right">
                        <i className="fa fa-weixin" id={`Tooltip-${searchTopicsItem.id}`}  aria-hidden="true"></i>
                        {" "}
                        {customAttributeAndCustomAttributeValueOccurencesCount}
                        <Tooltip placement="top" isOpen={this.state.isTooltipOpen} target={`Tooltip-${searchTopicsItem.id}`} toggle={this.handleToggleTooltip}>
                            {`Nalezeny ${customAttributeAndCustomAttributeValueOccurencesCount} odpovědi`}
                        </Tooltip>
                    </span>
            }
        </li>
    }
}

export default TopicSearchResultItem