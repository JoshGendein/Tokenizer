# Tokenizer

Finds TF-IDF for a set of documents (term frequency–inverse document frequency):

Equation for TF-IDF:

  TF-IDF = TF * LOG(N / DF)
  
  TF = frequency of token(i) in document(j)
  
  N = Total Document Count
  
  DF = Number of document containing token(i)
  
TF is used to scale the value up for the amount of times it appears in a document.

IDF is used to filter out words that appear in all or almost all documents.
