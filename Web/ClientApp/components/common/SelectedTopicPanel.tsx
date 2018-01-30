import * as React from "react";
import { ProductVariant } from "../../apimodels/ProductVariantsStore";
import { Topic } from "../../apimodels/TopicsStore";
import TopicPath from "./TopicPath";

interface SelectedTopicPanelProps {
    topics: Topic[];
    removeButtonClick: (topic: Topic) => void;
    isQuickSelection: boolean;
}

export const SelectedTopicPanel = ({ topics, removeButtonClick, isQuickSelection }: SelectedTopicPanelProps) => {
    const handleRemoveButtonClick = () => {
        const leafTopic = topics[topics.length - 1];
        removeButtonClick(leafTopic);
    };

    return (
        <tr>
            <td>
                <TopicPath topics={topics} />
                <i onClick={event => !isQuickSelection && handleRemoveButtonClick()} className={"fa fa-trash-o pointer float-right p-1 " + (isQuickSelection ? 'disabled' : '')} aria-hidden="true" />
            </td>
        </tr>
    );
};
