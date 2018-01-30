import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { initialize, getFormValues } from 'redux-form';
import { AppThunkAction } from './';
import config from '../configuration/config';
import { Topics } from './fakes/Topics';
import { getAncestorsAndMe, getLeaves, INode, flatSort, getFlattenNodes } from '../helpers/hierarchy';
import { Topic, ExistsPersonalTopicSelectionNameIM, PostPersonalTopicSelectionIM } from '../apimodels/TopicsStore';
import { TopicVM } from '../apimodels/Topics';
import { TopicSearchIM, TopicSearchVM, TopicSearchPair } from '../apimodels/Search';
import { CustomAttribute, CustomAttributeValue, ProductVariant } from '../apimodels/ProductMatrixStore';
import { Toastr } from './commonActionCreators/Toaster';
import { Dispatch } from 'react-redux';
import { findMatchedTopics, getSearchTopicsItems } from '../helpers/topics';
import { SuperAgentRequest } from 'superagent';

export interface SearchTopicsItem extends INode {
    id: number;
    children: SearchTopicsItem[];
    name?: string;
    displayName?: string;
    isChecked?: boolean;
    isIndeterminate?: boolean;
}

export interface SavedTopicsSelection {
    topicSelectionId: number;
    topicSelectionName: string;
}

export interface TopicState {
    topic: Topic;
    topicVM: TopicVM;
    parentTopicVM: TopicVM;
    topics: Topic[];
    selectedTopics: Topic[];
    visibleTopics: Topic[];
    topicsPathSelected: Topic[];
    savedTopicsSelections: SavedTopicsSelection[];
    searchTopicsItems: SearchTopicsItem[];
    topicOccurencesCountSum: number;
    customAttributeAndCustomAttributeValueOccurencesCountSum: number;
    topicSearchPairs: TopicSearchPair[];
}

export enum TopicsActionTypes {
    RequestTopic = 'REQUEST_TOPIC',
    ReceiveTopic = 'RECEIVE_TOPIC',
    RequestTopics = 'REQUEST_TOPICS',
    ReceiveTopics = 'RECEIVE_TOPICS',
    SelectTopic = 'SELECT_TOPIC',
    UnselectTopic = 'UNSELECT_TOPIC',
    SelectTopics = 'SELECT_TOPICS',
    UnselectTopics = 'UNSELECT_TOPICS',
    GetSavedTopicsSelections = 'GET_SAVED_TOPICS_SELECTIONS',
    RemoveSavedTopicsSelection = 'REMOVE_SAVED_TOPICS_SELECTION',
    GetSavedTopics = 'GET_SAVED_TOPICS',
    ShowChildrenTopics = 'SHOW_CHILDREN_TOPICS',
    GetFoundTopics = 'GET_FOUND_TOPICS',
    ReceiveFoundTopics = 'RECEIVE_FOUND_TOPICS',
    RequestSaveTopic = 'REQUEST_SAVE_TOPIC',
    ReceiveSaveTopic = 'RECEIVE_SAVE_TOPIC',
    RequestDeleteTopic = 'REQUEST_DELETE_TOPIC',
    ReceiveDeleteTopic = 'RECEIVE_DELETE_TOPIC',
    ClearTopic = 'CLEAR_TOPIC',
    AddSubtopic = 'ADD_SUBTOPIC'
}

interface ReceiveFoundTopics {
    type: TopicsActionTypes.ReceiveFoundTopics
    searchTopicsItems: SearchTopicsItem[];
    customAttributeAndCustomAttributeValueOccurencesCountSum: number;
    topicOccurencesCountSum: number;
    topicSearchPairs: TopicSearchPair[];
}

interface GetFoundTopics {
    type: TopicsActionTypes.GetFoundTopics;
    searchTopicsItems: SearchTopicsItem[];
}

interface RemoveSavedTopicsSelection {
    type: TopicsActionTypes.RemoveSavedTopicsSelection;
    removedTopicsSelectionId: number;
}

interface GetSavedTopics {
    type: TopicsActionTypes.GetSavedTopics;
    savedTopics: Topic[];
}

interface GetSavedTopicsSelections {
    type: TopicsActionTypes.GetSavedTopicsSelections;
    savedTopicsSelections: SavedTopicsSelection[];
}

interface RequestTopics {
    type: TopicsActionTypes.RequestTopics;
}

interface ReceiveTopics {
    type: TopicsActionTypes.ReceiveTopics;
    topics: Topic[];
}

interface RequestTopic {
    type: TopicsActionTypes.RequestTopic;
}

interface ReceiveTopic {
    type: TopicsActionTypes.ReceiveTopic;
    topicVM: TopicVM;
}

interface SelectTopic {
    type: TopicsActionTypes.SelectTopic;
    topic: Topic;
    topics: Topic[];
}

interface UnselectTopic {
    type: TopicsActionTypes.UnselectTopic;
    topic: Topic;
}

interface SelectTopics {
    type: TopicsActionTypes.SelectTopics;
    topics: Topic[];
}

interface UnselectTopics {
    type: TopicsActionTypes.UnselectTopics;
    topics: Topic[];
}

interface ShowChildrenTopcs {
    type: TopicsActionTypes.ShowChildrenTopics;
    ancestorsAndMe: Topic[];
    ancestorsAndMyChildren: Topic[];

}

interface RequestSaveTopic {
    type: TopicsActionTypes.RequestSaveTopic;
}

interface ReceiveSaveTopic {
    type: TopicsActionTypes.ReceiveSaveTopic;
    newTopicId: number;
}

interface RequestDeleteTopic {
    type: TopicsActionTypes.RequestDeleteTopic;
}

interface ReceiveDeleteTopic {
    type: TopicsActionTypes.ReceiveDeleteTopic;
}

interface ClearTopic {
    type: TopicsActionTypes.ClearTopic;
}

interface AddSubtopic {
    type: TopicsActionTypes.AddSubtopic;
    topicVM: TopicVM;
}

type TopicsAction = RequestTopics
    | RequestTopic
    | ReceiveTopic
    | ReceiveTopics
    | SelectTopic
    | UnselectTopic
    | SelectTopics
    | UnselectTopics
    | GetSavedTopicsSelections
    | GetSavedTopics
    | RemoveSavedTopicsSelection
    | ShowChildrenTopcs
    | GetFoundTopics
    | ReceiveFoundTopics
    | RequestSaveTopic
    | ReceiveSaveTopic
    | RequestDeleteTopic
    | ReceiveDeleteTopic
    | ClearTopic
    | AddSubtopic;

export const actionCreators = {
    searchTopicsAndAnswers: (searchText: string): AppThunkAction<TopicsAction> => (dispatch, getState, superagent, responseHandler) => {
        const { selectedProductVariants } = getState().productVariants;
        const productTypeId = getState().productTypes.productType.id;

        if(selectedProductVariants.length === 0) {
            Toastr.error({message: 'Nejsou vybrané žádné produkty'});
            return;
        }

        const searchTopicsRequestData: TopicSearchIM = {
            searchText,
            productVariantIds: selectedProductVariants.map(productVariant => productVariant.id)
        };

        superagent
            .post(`/api/topicsstore/producttypes/${productTypeId}/topics/search`)
            .send(searchTopicsRequestData)
            .then(responseHandler(response => {
                const { topicSearchPairs, topicOccurencesCountSum, customAttributeAndCustomAttributeValueOccurencesCountSum } = response.body as TopicSearchVM;
                const { topics, selectedTopics } = getState().topics;

                const foundTopics = getFlattenNodes(topics).filter(topic => {
                    if (topicSearchPairs.some(pair => topic.id === pair.topicId)) {
                        return topic;
                    }
                });

                const searchTopicsItems: SearchTopicsItem[] = getSearchTopicsItems(foundTopics, selectedTopics, topics);

                dispatch({
                    type: TopicsActionTypes.ReceiveFoundTopics,
                    topicSearchPairs,
                    customAttributeAndCustomAttributeValueOccurencesCountSum,
                    topicOccurencesCountSum,
                    searchTopicsItems
                });
            }))
            .catch(err => console.log(err.response.body));
    },
    searchTopics: (searchText: string): AppThunkAction<TopicsAction> => (dispatch, getState) => {
        const { topics, selectedTopics } = getState().topics;

        const foundTopics = findMatchedTopics(searchText, getFlattenNodes(topics));
        const searchTopicsItems: SearchTopicsItem[] = getSearchTopicsItems(foundTopics, selectedTopics, topics);

        dispatch({type: TopicsActionTypes.GetFoundTopics, searchTopicsItems })
    },
    selectFoundTopic: (topic: Topic): AppThunkAction<TopicsAction> => (dispatch: Function, getState) => {
        topic.children.length > 0
            ? dispatch(actionCreators.selectTopics(getLeaves(topic)))
            : dispatch(actionCreators.selectTopic(topic));

        const { searchTopicsItems, selectedTopics, topics } = getState().topics;
        const itemsIncludingSelectedItem: SearchTopicsItem[] = getSearchTopicsItems(searchTopicsItems, selectedTopics, topics);

        dispatch({type: TopicsActionTypes.GetFoundTopics, searchTopicsItems: itemsIncludingSelectedItem });
    },
    unselectFoundTopic: (topic: Topic): AppThunkAction<TopicsAction> => (dispatch: Function, getState) => {
        topic.children.length > 0
            ? dispatch(actionCreators.unselectTopics(getLeaves(topic)))
            : dispatch(actionCreators.unselectTopic(topic));

        const { searchTopicsItems, selectedTopics, topics } = getState().topics;
        const itemsIncludingUnselectedItem: SearchTopicsItem[] = getSearchTopicsItems(searchTopicsItems, selectedTopics, topics);

        dispatch({type: TopicsActionTypes.GetFoundTopics, searchTopicsItems: itemsIncludingUnselectedItem });
    },
    toggleAllFoundTopics: (): AppThunkAction<TopicsAction> => (dispatch: Function, getState) => {
        const { searchTopicsItems } = getState().topics;

        const leaves = searchTopicsItems.filter(item => item.children.length === 0) as Topic[];
        const hasAnySelectedTopics = searchTopicsItems.some(item => item.isChecked);

        hasAnySelectedTopics
            ? dispatch(actionCreators.unselectTopics(leaves))
            : dispatch(actionCreators.selectTopics(leaves));

        const { selectedTopics, topics } = getState().topics;
        const items: SearchTopicsItem[] = getSearchTopicsItems(searchTopicsItems, selectedTopics, topics);

        dispatch({type: TopicsActionTypes.GetFoundTopics, searchTopicsItems: items });
    },
    requestTopics: (productTypeId: number): AppThunkAction<TopicsAction> => (dispatch, getState, superagent, responseHandler) => {
        const request = superagent
            .get(`/api/topicsstore/producttypes/${productTypeId}/topics`)
            .then(responseHandler(response => dispatch({ type: TopicsActionTypes.ReceiveTopics, topics: response.body.topics })));

        addTask(request); // Ensure server-side prerendering waits for this to complete
        dispatch({ type: TopicsActionTypes.RequestTopics });
    },
    requestTopic: (formName:string, productTypeId: number, topicId: number): AppThunkAction<TopicsAction> => (dispatch, getState, superagent, responseHandler) => {
        dispatch({ type: TopicsActionTypes.RequestTopic });

        superagent.get(`/api/topicsstore/producttypes/${productTypeId}/topics/${topicId}`)
            .then(responseHandler(
                response => {
                    dispatch({ type: TopicsActionTypes.ReceiveTopic, topicVM: response.body });
                    dispatch(initialize(formName, response.body));
                }))
            .catch(reason => console.warn(reason));
    },
    selectTopic: (topic: Topic): AppThunkAction<TopicsAction> => (dispatch, getState, superagent, responseHandler) => {
        const { selectedTopics } = getState().topics;

        const topics = selectedTopics.concat(topic);
        const sortedTopics = topics.sort((a, b) => flatSort(a, b, getState().topics.topics));

        dispatch({ type: TopicsActionTypes.SelectTopic, topic, topics: sortedTopics });
    },
    saveTopicsSelection: (productTypeId: number, selectionName: string): AppThunkAction<TopicsAction> => (dispatch, getState, superagent, responseHandler) => {
        const { selectedTopics } = getState().topics;
        const existenceRequestData: ExistsPersonalTopicSelectionNameIM = {
            "topicSelectionName": selectionName
        }

        superagent
            .post(`/api/topicsstore/producttypes/${productTypeId}/topicselections/existence`)
            .send(existenceRequestData)
            .then(responseHandler(response => {
                const selectionAlreadyExists = response.body;
                const personalTopicSelectionRequestData: PostPersonalTopicSelectionIM = {
                    "topicSelectionName": selectionName,
                    "topics": selectedTopics.map(topic => topic.id)
                }
                let canBeSaved = true;

                if (selectionAlreadyExists) {
                    canBeSaved = false;
                    Toastr.confirm({
                        message: 'Výběr s tímto názvem již existuje. Přejete si jej přepsat?',
                        onOk: () => {
                            superagent
                                .post(`/api/topicsstore/producttypes/${productTypeId}/topicselections`)
                                .send(personalTopicSelectionRequestData)
                                .then(responseHandler(response => Toastr.success({ message: 'Výběr byl uložen.' })))
                        }
                    });
                }

                canBeSaved && superagent
                    .post(`/api/topicsstore/producttypes/${productTypeId}/topicselections`)
                    .send(personalTopicSelectionRequestData)
                    .then(responseHandler(response => Toastr.success({ message: 'Výběr byl uložen.' })));
            }))
            .then(responseHandler(response => Toastr.success({ message: 'Výběr byl uložen.' })))
            .catch(error => console.warn(error))
    },
    removeTopicsSelection: (productTypeId: number, selectionId: number): AppThunkAction<TopicsAction> => (dispatch, getState, superagent, responseHandler) => {
        Toastr.confirm({
            message: 'Přejete si výběr odebrat?',
            onOk: () => {
                superagent
                    .delete(`/api/topicsstore/producttypes/${productTypeId}/topicselections/${selectionId}`)
                    .then(responseHandler(response => {
                        dispatch({ type: TopicsActionTypes.RemoveSavedTopicsSelection, removedTopicsSelectionId: selectionId });
                        Toastr.success({message: 'Výběr byl odstraněn.'});
                    }));
            }
        });
    },
    loadTopicsFromSelection: (productTypeId: number, topicSelectionId: number): AppThunkAction<TopicsAction> => (dispatch, getState, superagent, responseHandler) => {
        superagent
            .get(`/api/topicsstore/producttypes/${productTypeId}/topicselections/${topicSelectionId}`)
            .then(responseHandler(response => {
                const { topics } = getState().topics;
                const savedTopicIds = response.body;

                // TODO: PK: these two lines are repeating - we should extract them into function
                const rootTopicsWithNoChildren = topics.filter(topic => topic.children.length === 0);
                const leaveTopics = topics.map(getLeaves).reduce((a, b) => a.concat(b), []).concat(rootTopicsWithNoChildren)

                dispatch({ type: TopicsActionTypes.GetSavedTopics, savedTopics: leaveTopics.filter(topic => savedTopicIds.some(savedTopic => savedTopic === topic.id)) });
            }));
    },
    loadTopicsSelections: (productTypeId: number): AppThunkAction<TopicsAction> => (dispatch, getState, superagent, responseHandler) => {
        superagent
            .get(`/api/topicsstore/producttypes/${productTypeId}/topicselections`)
            .then(responseHandler(response => dispatch({ type: TopicsActionTypes.GetSavedTopicsSelections, savedTopicsSelections: response.body as SavedTopicsSelection[] })));
    },
    unselectTopic: (topic: Topic): AppThunkAction<TopicsAction> => (dispatch, getState) => { console.log("unselect");
        const { topics, selectedTopics } = getState().topics;

        if (selectedTopics.find(topicItem => topicItem.id === topic.id)) {
            dispatch({ type: TopicsActionTypes.UnselectTopic, topic });
        }
    },
    selectTopics: (topics: Topic[]): AppThunkAction<TopicsAction> => (dispatch, getState) => {
        const { selectedTopics } = getState().topics;

        const topicsSet = new Set(topics)
        selectedTopics.map(selectedTopic => topicsSet.add(selectedTopic));

        const sortedTopics = Array.from(topicsSet).sort((a, b) => flatSort(a, b, getState().topics.topics));

        dispatch({ type: TopicsActionTypes.SelectTopics, topics: sortedTopics })
    },
    unselectTopics: (topics: Topic[]): AppThunkAction<TopicsAction> => (dispatch, getState) => {
        dispatch({ type: TopicsActionTypes.UnselectTopics, topics })
    },
    selectAllTopics: (): AppThunkAction<TopicsAction> => (dispatch, getState) => {
        const { topics } = getState().topics;

        const rootTopicsWithNoChildren = topics.filter(topic => topic.children.length === 0);
        const leaveTopics = topics.map(getLeaves).reduce((a, b) => a.concat(b), []).concat(rootTopicsWithNoChildren);

        dispatch({ type: TopicsActionTypes.SelectTopics, topics: leaveTopics })
    },
    unselectAllTopics: (): AppThunkAction<TopicsAction> => (dispatch, getState) => {
        const { topics } = getState().topics;

        const rootTopicsWithNoChildren = topics.filter(topic => topic.children.length === 0);
        const leaveTopics = topics.map(getLeaves).reduce((a, b) => a.concat(b), []).concat(rootTopicsWithNoChildren);

        dispatch({ type: TopicsActionTypes.UnselectTopics, topics: leaveTopics })
    },
    showChildrenTopics: (topic: Topic): AppThunkAction<TopicsAction> => (dispatch, getState) => {
        const topics = getState().topics.topics;
        const ancestorsAndMe = getAncestorsAndMe(topic, topics);
        const ancestorsAndMyChildren = ancestorsAndMe.map(ancestor => ancestor.children).reduce((a, b) => a.concat(b));

        dispatch({ type: TopicsActionTypes.ShowChildrenTopics, ancestorsAndMe, ancestorsAndMyChildren })
    },
    saveTopic: (formName: string, productTypeId: number, callback: () => void): AppThunkAction<TopicsAction> => (dispatch, getState, superagent, responseHandler) => {
        dispatch({ type: TopicsActionTypes.RequestSaveTopic });

        const topicId = getState().topics.topicVM.id;

        const inputModel = getFormValues(formName)(getState());

        let request: SuperAgentRequest;

        if (topicId === null) {
            request = superagent.post(`/api/topicsstore/producttypes/${productTypeId}/topics`);
            inputModel["parentTopicId"] = getState().topics.parentTopicVM.id;
        } else {
            request = superagent.put(`/api/topicsstore/producttypes/${productTypeId}/topics/${topicId}`);
        }

        request
            .send(inputModel)
            .then(responseHandler(
                response => {
                    dispatch({ type: TopicsActionTypes.ReceiveSaveTopic, newTopicId: response.body });
                    Toastr.success({ message: "Téma bylo uloženo." });
                    callback(); // TODO: technical debt: callbacks are pure evil within redux environment
                }))
            .catch(reason => console.warn(reason));
    },
    deleteTopic: (productTypeId: number, callback: () => void): AppThunkAction<TopicsAction> => (dispatch, getState, superagent, responseHandler) => {
        const topic = getState().topics.topicVM;
        if (topic.id !== null) {
            dispatch({ type: TopicsActionTypes.RequestDeleteTopic });

            Toastr.confirm({
                message: `Opravdu chcete smazat téma '${topic.name}'?`,
                onOk: () => superagent.delete(`/api/topicsstore/producttypes/${productTypeId}/topics/${topic.id}`)
                    .then(responseHandler(
                        response => {
                            dispatch({ type: TopicsActionTypes.ReceiveDeleteTopic });
                            Toastr.success({ message: "Téma bylo smazáno" });
                            callback();
                        }))
                    .catch(reason => console.warn(reason))
            })            
        }
    },
    clearTopic: (): AppThunkAction<TopicsAction> => (dispatch, getState, superagent, responseHandler) => dispatch({ type: TopicsActionTypes.ClearTopic }),
    addSubtopic: (formName: string): AppThunkAction<TopicsAction> => (dispatch, getState, superagent, responseHandler) => {
        const newTopic = {
            id: null,
            hasChildren: false,
            hasCustomAttributes: false,
            name: null,
            validFrom: null,
            validTo: null
        }
        dispatch(initialize(formName, newTopic));
        dispatch({ type: TopicsActionTypes.AddSubtopic, topicVM: newTopic })
    }
}

const unloadedState: TopicState = {
    topic: null, topicVM: null, parentTopicVM: null, topics: [], selectedTopics: [], savedTopicsSelections: [], visibleTopics: [], topicsPathSelected: [], searchTopicsItems: [],
    topicOccurencesCountSum: 0, customAttributeAndCustomAttributeValueOccurencesCountSum: 0, topicSearchPairs: [] };

export const reducer: Reducer<TopicState> = (state: TopicState, action: TopicsAction) => {
    switch (action.type) {
        case TopicsActionTypes.ReceiveFoundTopics:
            return {
                ...state,
                searchTopicsItems: action.searchTopicsItems,
                topicOccurencesCountSum: action.topicOccurencesCountSum,
                customAttributeAndCustomAttributeValueOccurencesCountSum: action.customAttributeAndCustomAttributeValueOccurencesCountSum,
                topicSearchPairs: action.topicSearchPairs
            };
        case TopicsActionTypes.ReceiveSaveTopic:
            if (action.newTopicId) {
            return {
                ...state, topicVM: { ...state.topicVM, id: action.newTopicId }
                }
            }
        case TopicsActionTypes.RequestTopic:
        case TopicsActionTypes.RequestTopics:
        case TopicsActionTypes.RequestSaveTopic:        
        case TopicsActionTypes.RequestDeleteTopic:
            return {...state};
        case TopicsActionTypes.ReceiveTopic:
            return {
                ...state,
                topicVM: action.topicVM,
                parentTopicVM: action.topicVM
            };
        case TopicsActionTypes.ReceiveTopics:
            return {
                ...state,
                topics: action.topics,
                visibleTopics: action.topics
            };
        case TopicsActionTypes.SelectTopic:
            return {
                ...state,
                topic: action.topic,
                selectedTopics: action.topics
            };
        case TopicsActionTypes.UnselectTopic:
            return {
                ...state,
                selectedTopics: state.selectedTopics.filter(topicItem => topicItem.id !== action.topic.id)
            };
        case TopicsActionTypes.SelectTopics:
            return {
                ...state,
                selectedTopics: action.topics
            };
        case TopicsActionTypes.UnselectTopics:
            return {
                ...state,
                selectedTopics: state.selectedTopics.filter(topicItem => action.topics.filter(newTopic => newTopic.id === topicItem.id).length === 0)
            };
        case TopicsActionTypes.GetSavedTopicsSelections:
            return {
                ...state,
                savedTopicsSelections: action.savedTopicsSelections
            };
        case TopicsActionTypes.GetSavedTopics:
            return {
                ...state,
                selectedTopics: action.savedTopics
            };
        case TopicsActionTypes.RemoveSavedTopicsSelection:
            return {
                ...state,
                savedTopicsSelections: state.savedTopicsSelections.filter(selection => selection.topicSelectionId !== action.removedTopicsSelectionId)
            };
        case TopicsActionTypes.GetFoundTopics:
            return {
                ...state,
                searchTopicsItems: action.searchTopicsItems
            };
        case TopicsActionTypes.ShowChildrenTopics:
            return {
                ...state,
                visibleTopics: action.ancestorsAndMyChildren,
                topicsPathSelected: action.ancestorsAndMe
            };
        case TopicsActionTypes.ClearTopic:
        case TopicsActionTypes.ReceiveDeleteTopic:
            return { ...state, topicVM: null, parentTopicVM: null };

        case TopicsActionTypes.AddSubtopic:
            return {
                ...state, topicVM: action.topicVM
            }
        default:
            // The following line guarantees that every action in the SegmentsAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    return state || unloadedState;
};
