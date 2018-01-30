import * as React from 'react';
import { Alert } from 'reactstrap';
import TopicSearchResultItem from './TopicSearchResultItem';
import { Topic, TopicSearchPair } from '../../../apimodels/TopicsStore';
import { SearchTopicsItem } from '../../../store/Topics';

interface TopicSearchResultListProps {
    searchTopicsItems: SearchTopicsItem[];
    searchedText: string;
    toggleAllFoundTopics: () => void;
    unselectFoundTopic: (topic: Topic) => void;
    selectFoundTopic: (topic: Topic) => void;
    topicOccurencesCountSum: number;
    customAttributeAndCustomAttributeValueOccurencesCountSum: number;
    isSearchAnswersModeOn: boolean;
    topicSearchPairs: TopicSearchPair[];
}

const TopicSearchResultList = (props: TopicSearchResultListProps) => {
    const {searchTopicsItems, searchedText, toggleAllFoundTopics, customAttributeAndCustomAttributeValueOccurencesCountSum,
        topicOccurencesCountSum, isSearchAnswersModeOn, topicSearchPairs} = props;

    const hasAnySelectedTopics = searchTopicsItems.some(item => item.isChecked);

    const getOccucrencesCountSums = (pairs: TopicSearchPair[], searchTopicsItem: SearchTopicsItem): number => {
        const currentPair = topicSearchPairs.find(pair => pair.topicId === searchTopicsItem.id);
        return currentPair ? currentPair.customAttributeAndCustomAttributeValueOccurencesCount : 0;
    }

    const handleSelectControlClick = () => {
        toggleAllFoundTopics();
    }

    return <div className="mt-4">
        {
            isSearchAnswersModeOn
                && <Alert color="primary">
                    {`Pro "${searchedText}" byly nalezeny ${topicOccurencesCountSum} názvy témat a ${customAttributeAndCustomAttributeValueOccurencesCountSum} odpovědi.`}
                    <span className="ml-5">
                        Odpovědi jsou označeny ikonou
                        {' '}
                        <i className="fa fa-weixin" aria-hidden="true"></i>
                    </span>
                </Alert>
        }
        <span>Nalezená témata</span>
        <span className="ml-3 topic-search-active-text" onClick={handleSelectControlClick}>
            {
                hasAnySelectedTopics
                    ? "Zrušit výběr nalezených"
                    : "Označit všechna nalezená"
            }
        </span>
        <div className="mt-2">
            {
                searchTopicsItems.length > 0
                    ? <ul className="topic-search-result-list">
                        {
                            searchTopicsItems.map(searchTopicsItem =>
                                <TopicSearchResultItem
                                    {...props}
                                    searchTopicsItem={searchTopicsItem}
                                    key={searchTopicsItem.id}
                                    customAttributeAndCustomAttributeValueOccurencesCount={getOccucrencesCountSums(topicSearchPairs, searchTopicsItem)} />
                            )
                        }
                    </ul>
                    : <Alert color="danger">
                        {`K řetězci "${searchedText}" nebylo nic nalezeno. Zkuste to jinak.`}
                    </Alert>
            }
        </div>
    </div>
}

export default TopicSearchResultList