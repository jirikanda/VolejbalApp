import * as React from 'react';
import { Button, Input, InputGroup, InputGroupButton } from 'reactstrap';
import TopicSearchResultList from './TopicSearchResultList';
import { Topic, TopicSearchPair } from '../../../apimodels/TopicsStore';
import Topics from "../../../containers/topics/Topics";
import { SearchTopicsItem } from '../../../store/Topics';
import { ProductVariant } from '../../../apimodels/ProductMatrixStore';

interface TopicSearchProps {
    toggleAllFoundTopics: () => void;
    unselectFoundTopic: (topic: Topic) => void;
    selectFoundTopic: (topic: Topic) => void;
    searchTopics: (searchedText: string) => void;
    searchTopicsAndAnswers: (searchText: string) => void;
    searchTopicsItems: SearchTopicsItem[];
    selectedProductTypeId: number;
    isSearchingAllowed: boolean;
    topicOccurencesCountSum: number;
    customAttributeAndCustomAttributeValueOccurencesCountSum: number;
    topicSearchPairs: TopicSearchPair[];
}

interface TopicSearchState {
    searchModeOn: boolean;
}

class TopicSearch extends React.Component<TopicSearchProps, TopicSearchState> {
    searchAnswersCheckboxRef: HTMLInputElement;
    searchInputRef: HTMLInputElement;
    backToTopicsList: boolean;

    constructor(props: TopicSearchProps) {
        super(props);

        this.state = {
            searchModeOn: false
        }
        this.backToTopicsList = false;
    }

    isSearchAnswersModeOn = () => this.searchAnswersCheckboxRef.checked;

    getSearchFunction = (searchAnswers: boolean) => searchAnswers ? this.props.searchTopicsAndAnswers : this.props.searchTopics;

    handleSearchButtonClick = () => {
        this.getSearchFunction(this.isSearchAnswersModeOn())(this.searchInputRef.value);

        if (!this.props.isSearchingAllowed && this.isSearchAnswersModeOn()) {
            return;
        }

        this.setState(() => ({
            searchModeOn: true
        }));
    }

    handleBackToTopicsList = () => {
        this.searchInputRef.value = "";
        this.backToTopicsList = true;
        this.setState(() => ({
            searchModeOn: false
        }));
    }

    render() {
        const { searchModeOn } = this.state;

        return <div className="topic-search mb-2">
            <div className="w-50">
                <InputGroup>
                    <input type="text" ref={ref => this.searchInputRef = ref} className="topic-search-input form-control" />
                    <InputGroupButton className='input-group-append'>
                        <Button color="primary" outline className="" onClick={this.handleSearchButtonClick}>Hledat</Button>
                    </InputGroupButton>
                </InputGroup>
                {
                    searchModeOn && <Button color="primary" outline className="pull-right" onClick={this.handleBackToTopicsList}>Zpět na výpis témat</Button>
                }
            </div>
            <div>
                <input type="checkbox" name="search-answers-checkbox" ref={ref => this.searchAnswersCheckboxRef = ref} className="form-control topic-search-answer-checkbox pointer" />
                <label htmlFor="search-answers-checkbox" className="ml-1">Hledat i v odpovědích</label>
            </div>
            {
                searchModeOn
                    ? <TopicSearchResultList
                        {...this.props}
                        isSearchAnswersModeOn={this.isSearchAnswersModeOn()}
                        searchedText={this.searchInputRef.value} />
                    : <Topics
                        {...this.props}
                        shouldPreserveSelection={(() => {
                            const result = this.backToTopicsList;
                            this.backToTopicsList = false;
                            return result
                        })()} />
            }
        </div>
    }
}

export default TopicSearch